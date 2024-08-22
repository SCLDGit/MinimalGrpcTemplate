using Grpc.Core;

using Microsoft.Extensions.Logging;

using MinimalGrpcTemplate.Api.Protos.V1.Microservice;

namespace MinimalGrpcTemplate.MicroService.Services.GrpcServiceImplementations;

internal class MicroserviceServiceImplementation(ILogger<MicroserviceServiceImplementation> c_logger) : Microservice.MicroserviceBase
{
    public override async Task<G_AddNumbersServiceResponse> ProcessAddNumbers(G_AddNumbersServiceRequest p_request, ServerCallContext p_context)
    {
        c_logger.LogInformation("Received request to add numbers: {Numbers}", p_request.Numbers);
        
        var sum = p_request.Numbers.Sum();
        
        return await Task.FromResult(new G_AddNumbersServiceResponse
                              {
                                  Result = sum
                              });
    }
}