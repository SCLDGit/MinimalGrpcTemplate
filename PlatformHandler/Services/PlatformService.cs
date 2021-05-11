using System.Threading.Tasks;
using DevExpress.Xpo;
using Grpc.Core;
using OrchestratorService;
using PlatformService;


namespace PlatformHandler.Services
{
    public class PlatformService : Platform.PlatformBase
    {
        //public Orchestrator.OrchestratorClient OrchestratorClient { get; } = new(new Channel("localhost:30052", ChannelCredentials.Insecure));

        public PlatformService()
        {
            UoW = new UnitOfWork();
            UoWLock = new object();
        }

        public UnitOfWork UoW { get; }
        public object UoWLock { get; }

        public override Task<G_PlatformRequestResponse> GetPlatform(G_ScanRequest request, ServerCallContext context)
        {
            var test = request.PolicyItemName;
            var test1 = request.OriginId;
            //OrchestratorClient.EnqueueRequest(new G_OrchestrationRequest()
            //{
            //    OriginId = request.OriginId,
            //    Response = new G_Response()
            //    {
            //        WasSuccessful = true,
            //        Response = "GetPlatform added successfully"
            //    }
            //});


            var result = string.Empty;
            switch (request.PolicyItemName.ToUpper())
            {
                case "WIN":
                    result = "WIN";
                    break;
                case "LINUX":
                    result = "LINUX";
                    break;
            }

            

            return Task.FromResult(new G_PlatformRequestResponse()
            {
                Response = $"{result}"
            });
        }
    }
}
