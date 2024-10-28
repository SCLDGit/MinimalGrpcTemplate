using System.Net;

using Grpc.Core;
using Grpc.Net.Client;
using Grpc.Net.Client.Configuration;

namespace MinimalGrpcTemplate.Client.Services.ServerInteraction;

internal class ServerConnectionInfo
{
    internal  GrpcChannel Channel           { get; private set; } = GrpcChannel.ForAddress("https://localhost:5005");
    private string      ConnectionAddress { get; set; }         = string.Empty;

    public void SetUnauthenticatedChannel(string p_target, ushort p_port)
    {
        Channel.Dispose();
        
        var hostAddress = GetConnectionAddress(p_target, p_port);

        ConnectionAddress = hostAddress;

        var credentials = CallCredentials.FromInterceptor((_, _) => Task.CompletedTask);
        
        var handler = new SocketsHttpHandler
                      {
                          PooledConnectionIdleTimeout    = Timeout.InfiniteTimeSpan,
                          KeepAlivePingDelay             = TimeSpan.FromSeconds(60),
                          KeepAlivePingTimeout           = TimeSpan.FromSeconds(30),
                          KeepAlivePingPolicy            = HttpKeepAlivePingPolicy.Always,
                          EnableMultipleHttp2Connections = true
                      };

        var methodConfig = new MethodConfig
                           {
                               Names = { MethodName.Default },
                               RetryPolicy = new RetryPolicy
                                             {
                                                 MaxAttempts          = 3,
                                                 InitialBackoff       = TimeSpan.FromSeconds(1),
                                                 MaxBackoff           = TimeSpan.FromSeconds(2.5),
                                                 BackoffMultiplier    = 1.5,
                                                 RetryableStatusCodes = { StatusCode.Unavailable }
                                             }
                           };

        Channel = GrpcChannel.ForAddress(hostAddress, new GrpcChannelOptions
                                                      {
                                                          HttpHandler = handler,
                                                          Credentials =
                                                              ChannelCredentials.Create(new SslCredentials(),
                                                                                        credentials),
                                                          ServiceConfig = new ServiceConfig
                                                                          {
                                                                              MethodConfigs = { methodConfig }
                                                                          }
                                                      });
    }

    private static string GetConnectionAddress(string p_target, ushort p_port)
    {
        if ( p_target.Equals("localhost", StringComparison.InvariantCultureIgnoreCase) ||
             IPAddress.TryParse(p_target, out _) )
        {
            return $"https://{p_target}:{p_port}";
        }

        var hostAddresses = Dns.GetHostEntry(p_target);
        
        return $"dns:///{hostAddresses.HostName}:{p_port}";
    }

    public void UpgradeToSecuredChannel(string p_userName, string p_token)
    {
        Channel.Dispose();

        var credentials = CallCredentials.FromInterceptor((_, p_metadata) =>
                                                          {
                                                              p_metadata.Add("Authorization", $"Bearer {p_token}");
                                                              p_metadata.Add("UserName", p_userName);
                                                              return Task.CompletedTask;
                                                          });

        var handler = new SocketsHttpHandler
                      {
                          PooledConnectionIdleTimeout    = Timeout.InfiniteTimeSpan,
                          KeepAlivePingDelay             = TimeSpan.FromSeconds(60),
                          KeepAlivePingTimeout           = TimeSpan.FromSeconds(30),
                          KeepAlivePingPolicy            = HttpKeepAlivePingPolicy.Always,
                          EnableMultipleHttp2Connections = true
                      };

        var methodConfig = new MethodConfig
                           {
                               Names = { MethodName.Default },
                               RetryPolicy = new RetryPolicy
                                             {
                                                 MaxAttempts          = 5,
                                                 InitialBackoff       = TimeSpan.FromSeconds(1),
                                                 MaxBackoff           = TimeSpan.FromSeconds(5),
                                                 BackoffMultiplier    = 1.5,
                                                 RetryableStatusCodes = { StatusCode.Unavailable }
                                             }
                           };

        Channel = GrpcChannel.ForAddress(ConnectionAddress, new GrpcChannelOptions
                                                            {
                                                                HttpHandler = handler,
                                                                Credentials =
                                                                    ChannelCredentials.Create(new SslCredentials(),
                                                                                              credentials),
                                                                ServiceConfig = new ServiceConfig
                                                                                {
                                                                                    MethodConfigs = { methodConfig }
                                                                                }
                                                            });
    }
}