using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DevExpress.DirectX.Common.Direct2D;
using DevExpress.Xpo;
using Grpc.Core;
using OrchestratorService;
using PlatformService;
using SteelCloud.CommonEnumerations;
using SteelCloud.Moonshot.Db;


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
        public virtual RuntimeCollection RuntimeCollection { get; set; }
        public virtual List<PolicyVersionItem> RuntimePolicyVersionItems { get; set; } = new List<PolicyVersionItem>();

        public override Task<G_PlatformRequestResponse> GetPlatform(G_ScanRequest request, ServerCallContext context)
        {
            //
            //LoadDB();
            //OrchestratorClient.EnqueueRequest(new G_OrchestrationRequest()
            //{
            //    OriginId = request.OriginId,
            //    Response = new G_Response()
            //    {
            //        WasSuccessful = true,
            //        Response = "GetPlatform added successfully"
            //    }
            //});

            var endpoint = UoW.Query<Endpoint>().FirstOrDefault(p_o => p_o.Address == request.EndPointAddress);
            RuntimeCollection = UoW.Query<RuntimeCollection>().FirstOrDefault(p_o => p_o.Parent == endpoint && p_o.PolicyCollection.Name.ToUpper() == "COLL1");
            var runtimePolicy = RuntimeCollection.RuntimePolicies.FirstOrDefault(p_o => p_o.Policy.Name.ToUpper() == request.PolicyItemName && p_o.Policy.Version.ToUpper() == request.PolicyItenVersion);

            var os = (enmSystemTargetType) runtimePolicy.Policy.Parent.SystemTargetType;
            var result = string.Empty;
            if (os.HasFlag(enmSystemTargetType.WINDOWS))
            {
                result = "WIN";
            }
            else if (os.HasFlag(enmSystemTargetType.LINUX))
            {
                result = "LINUX";
            }

            return Task.FromResult(new G_PlatformRequestResponse()
            {
                Response = $"{result}"
            });
        }

        public void LoadDB()
        {
            var group = new Group(UoW);
            group.Name = "MyGroup";
            //
            var endpoint = new Endpoint(UoW);
            endpoint.Address = "10.1.1.1";
            endpoint.Name = "MyEndpoint";
            //
            group.Children.Add(endpoint);
            //
            var runtimePolicy = new RuntimePolicy(UoW);
            runtimePolicy.Policy = UoW.Query<PolicyVersionItem>().FirstOrDefault(p_o =>
                p_o.Name == "MS_WIN_10_V1709_V2R1_STIG_DOMAIN" && p_o.Version == "1.0");
            //
            var policyCollectionVersionItem =
                UoW.Query<PolicyCollectionVersionItem>().FirstOrDefault(p_o => p_o.Name == "coll1" && p_o.Version == "0.1");
            var runtimeCollection = new RuntimeCollection(UoW);
            runtimeCollection.PolicyCollection = policyCollectionVersionItem;
            runtimeCollection.RuntimePolicies.Add(runtimePolicy);
            //
            endpoint.RuntimeCollections.Add(runtimeCollection);
            UoW.CommitChanges();
        }
    }
}
