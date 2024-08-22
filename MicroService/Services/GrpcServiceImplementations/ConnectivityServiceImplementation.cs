using Grpc.Core;

using Microsoft.Extensions.Logging;

using MinimalGrpcTemplate.Api.Protos.V1.Connectivity;

using Serilog;

namespace MinimalGrpcTemplate.MicroService.Services.GrpcServiceImplementations;

internal class ConnectivityServiceImplementation(ILogger<ConnectivityServiceImplementation> c_logger) : Connectivity.ConnectivityBase
{
    public override async Task<G_ConnectionCheckResponse> CheckServerConnection(G_ConnectionCheckRequest p_request, ServerCallContext p_context)
    {
        c_logger.LogInformation("Received request to check server connection");
        
        return await Task.FromResult(new G_ConnectionCheckResponse());
    }
}