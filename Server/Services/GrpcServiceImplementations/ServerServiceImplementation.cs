using Grpc.Core;

using Microsoft.Extensions.Logging;

using MinimalGrpcTemplate.Api.Protos.V1.Microservice;
using MinimalGrpcTemplate.Api.Protos.V1.Server;
using MinimalGrpcTemplate.Server.Services.MicroserviceInteraction;

namespace MinimalGrpcTemplate.Server.Services.GrpcServiceImplementations;

public class ServerServiceImplementation(ILogger<ServerServiceImplementation> c_logger,
                                         IConnectionInfoService               c_connectionInfoService) : Api.Protos.V1.Server.Server.ServerBase
{
    public override async Task<G_AddNumbersResponse> AddNumbers(G_AddNumbersRequest p_request, ServerCallContext p_context)
    {
        c_logger.LogInformation("Add numbers requested by client");

        var client = new Microservice.MicroserviceClient(c_connectionInfoService.Channel);

        var response = await client.ProcessAddNumbersAsync(new G_AddNumbersServiceRequest
                                                           {
                                                               Numbers = { p_request.Numbers }
                                                           });

        return new G_AddNumbersResponse
               {
                   Result = response.Result
               };
    }
}