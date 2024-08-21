using Grpc.Core;

using Microsoft.Extensions.Logging;

using MinimalGrpcTemplate.Api.Protos.V1.Connectivity;

namespace MinimalGrpcTemplate.Server.Services.GrpcServiceImplementations;

internal class ConnectivityServiceImplementation(ILogger<ConnectivityServiceImplementation> c_logger) : Connectivity.ConnectivityBase
{
    public override async Task<G_ConnectionCheckResponse> CheckServerConnection(G_ConnectionCheckRequest p_request, ServerCallContext p_context)
    {
        c_logger.LogInformation("Server connection check requested by client");
        
        return await Task.FromResult(new G_ConnectionCheckResponse());
    }
}