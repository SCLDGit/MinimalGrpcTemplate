using System.Net;
using System.Net.Sockets;

using Microsoft.Extensions.Logging;

namespace MinimalGrpcTemplate.Server.Services.MicroserviceInteraction;

internal class SocketConnectionFactoryService(ILogger<SocketConnectionFactoryService> c_logger)
{
    private EndPoint? m_endpoint;
    
    internal void SetEndpoint(EndPoint p_endpoint)
    {
        m_endpoint = p_endpoint;
    }
    
    internal async ValueTask<Stream> ConnectAsync(SocketsHttpConnectionContext _,
                                                  CancellationToken            p_cancellationToken = default)
    {
        if ( m_endpoint is null )
        {
            throw new InvalidOperationException("Endpoint not set");
        }

        c_logger.LogInformation("Connecting to microservice");
        
        var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);

        try
        {
            await socket.ConnectAsync(m_endpoint, p_cancellationToken).ConfigureAwait(false);
            return new NetworkStream(socket, true);
        }
        catch
        {
            socket.Dispose();
            throw;
        }
    }
}