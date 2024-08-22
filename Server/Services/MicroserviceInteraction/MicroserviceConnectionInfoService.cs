using System.Net.Sockets;

using Grpc.Net.Client;

using Microsoft.Extensions.Logging;

using MinimalGrpcTemplate.Server.Global.IO.Files;

using Serilog;

namespace MinimalGrpcTemplate.Server.Services.MicroserviceInteraction;

internal class MicroserviceConnectionInfoService : IConnectionInfoService
{
    private readonly SocketConnectionFactoryService             m_socketService;
    private readonly ILogger<MicroserviceConnectionInfoService> m_logger;
    
    public MicroserviceConnectionInfoService(ILogger<MicroserviceConnectionInfoService> p_logger,
                                             SocketConnectionFactoryService             p_socketService)
    {
        m_socketService = p_socketService;
        m_logger   = p_logger;

        Channel = CreateChannel();
    }
    
    public GrpcChannel? Channel { get; }
    
    private GrpcChannel CreateChannel()
    {
        m_logger.LogInformation("Creating channel for microservice connection");
        
        var udsEndPoint       = new UnixDomainSocketEndPoint(ApplicationFiles.SocketFile);
        
        m_socketService.SetEndpoint(udsEndPoint);
        
        var socketsHttpHandler = new SocketsHttpHandler
                                 {
                                     ConnectCallback = m_socketService.ConnectAsync
                                 };

        return GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions
                                                          {
                                                              HttpHandler = socketsHttpHandler
                                                          });
    }
}