using Grpc.Net.Client;

namespace MinimalGrpcTemplate.Server.Services.MicroserviceInteraction;

public interface IConnectionInfoService
{ 
    GrpcChannel? Channel { get; }
}