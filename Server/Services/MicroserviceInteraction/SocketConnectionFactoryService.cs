using System.Net;
using System.Net.Sockets;

namespace MinimalGrpcTemplate.Server.Services.MicroserviceInteraction;

internal class SocketConnectionFactoryService
{
    private EndPoint m_endpoint;
    
    internal void SetEndpoint(EndPoint p_endpoint)
    {
        m_endpoint = p_endpoint;
    }
    
    internal async ValueTask<Stream> ConnectAsync(SocketsHttpConnectionContext _,
                                                  CancellationToken            p_cancellationToken = default)
    {
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