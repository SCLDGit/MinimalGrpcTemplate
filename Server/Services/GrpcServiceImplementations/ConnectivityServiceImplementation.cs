using Grpc.Core;

using MinimalGrpcTemplate.Api.Protos.V1.Connectivity;

namespace MinimalGrpcTemplate.Server.Services.GrpcServiceImplementations;

internal class ConnectivityServiceImplementation : Connectivity.ConnectivityBase
{
    public override async Task<G_ConnectionCheckResponse> CheckServerConnection(G_ConnectionCheckRequest p_request, ServerCallContext p_context)
    {
        return await Task.FromResult(new G_ConnectionCheckResponse());
    }
}