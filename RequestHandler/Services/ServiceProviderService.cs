using System.Threading.Tasks;
using Grpc.Core;
using OrchestratorService;
using RequestHandlerService;

namespace RequestHandler.Services
{
    public class HandlerService : Handler.HandlerBase
    {   
        public Orchestrator.OrchestratorClient OrchestratorClient { get; } = new (new Channel("localhost:30052", ChannelCredentials.Insecure));
        
        public override Task<G_RequestResponse> AddItem1(G_RequestType1 request, ServerCallContext context)
        {
            OrchestratorClient.EnqueueRequest(new G_OrchestrationRequest()
            {
                OriginId = request.OriginId,
                Response = new G_Response()
                {
                    WasSuccessful = true,
                    Response = "Item Type 1 Added Successfully!"
                }
            });
            
            return Task.FromResult(new G_RequestResponse()
            {
                Response = "Handler response type 1..."
            });
        }
        
        public override Task<G_RequestResponse> AddItem2(G_RequestType2 request, ServerCallContext context)
        {
            OrchestratorClient.EnqueueRequest(new G_OrchestrationRequest()
            {
                OriginId = request.OriginId,
                Response = new G_Response()
                {
                    WasSuccessful = true,
                    Response = "Item Type 2 Added Successfully!"
                }
            });            
            return Task.FromResult(new G_RequestResponse()
            {
                Response = "Handler response type 2..."
            });
        }
    }
}