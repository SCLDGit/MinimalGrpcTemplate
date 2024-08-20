using System.Net.Sockets;

using Grpc.Net.Client;

using MinimalGrpcTemplate.Server.Global.IO.Files;

namespace MinimalGrpcTemplate.Server.Services.MicroserviceInteraction;

internal class MicroserviceConnectionInfoService : IConnectionInfoService
{
    private readonly SocketConnectionFactoryService m_socketService;
    
    public MicroserviceConnectionInfoService(SocketConnectionFactoryService p_socketService)
    {
        m_socketService = p_socketService;
        
        Channel = CreateChannel();
    }
    
    public GrpcChannel? Channel { get; }
    
    private GrpcChannel CreateChannel()
    {
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