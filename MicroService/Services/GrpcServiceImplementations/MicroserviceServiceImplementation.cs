using Grpc.Core;

using MinimalGrpcTemplate.Api.Protos.V1.Microservice;

namespace MinimalGrpcTemplate.MicroService.Services.GrpcServiceImplementations;

internal class MicroserviceServiceImplementation : Microservice.MicroserviceBase
{
    public override async Task<G_AddNumbersServiceResponse> ProcessAddNumbers(G_AddNumbersServiceRequest p_request, ServerCallContext p_context)
    {
        var sum = p_request.Numbers.Sum();
        
        return await Task.FromResult(new G_AddNumbersServiceResponse
                              {
                                  Result = sum
                              });
    }
}