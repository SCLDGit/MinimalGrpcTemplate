using DevExpress.Xpo;

using Grpc.Core;

using Microsoft.Extensions.Logging;

using MinimalGrpcTemplate.Api.Protos.V1.Microservice;
using MinimalGrpcTemplate.Api.Protos.V1.Server;
using MinimalGrpcTemplate.Server.Services.MicroserviceInteraction;
using MinimalGrpcTemplate.Server.Services.UserManagement;

namespace MinimalGrpcTemplate.Server.Services.GrpcServiceImplementations;

public class ServerServiceImplementation(ILogger<ServerServiceImplementation> c_logger,
                                         IConnectionInfoService               c_connectionInfoService,
                                         UserManagementService                c_userManagementService) : Api.Protos.V1.Server.Server.ServerBase
{
    public override async Task<G_AddNumbersResponse> AddNumbers(G_AddNumbersRequest p_request, ServerCallContext p_context)
    {
        c_logger.LogInformation("Add numbers requested by client");
        
        var user  = c_userManagementService.CreateUser("MattIsAwesome");
        var test  = user.Session as UnitOfWork;
        await test.CommitChangesAsync();
        var user2 = c_userManagementService.CreateUser("JamieTurn");
        c_userManagementService.SaveUser(user);

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
    
    
    public override async Task<G_AddNumbersResponse> AddNumbers2(G_AddNumbersRequest p_request, ServerCallContext p_context)
    {
        c_logger.LogInformation("Add numbers requested by client");
        
        var user3 = c_userManagementService.CreateUser("JeremyCrab");
        //c_userManagementService.SaveUser(user3);

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