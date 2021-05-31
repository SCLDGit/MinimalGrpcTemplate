using System;
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
using ScanWinService;
using DatabaseService;


namespace PlatformHandler.Services
{
    public class PlatformService : Platform.PlatformBase
    {
        public PlatformService()
        {
            UoW = new UnitOfWork();
            ScanWinHandler = new ScanWin.ScanWinClient(new Channel("localhost:30055", ChannelCredentials.Insecure));
            dbServiceHandler = new DbService.DbServiceClient(new Channel("localhost:30057", ChannelCredentials.Insecure));
        }

        public UnitOfWork UoW { get; }
        public virtual RuntimeCollection RuntimeCollection { get; set; }
        public virtual List<PolicyVersionItem> RuntimePolicyVersionItems { get; set; } = new List<PolicyVersionItem>();
        private ScanWin.ScanWinClient ScanWinHandler { get; }
        private DbService.DbServiceClient dbServiceHandler { get; }
        

        public override Task<G_PlatformRequestResponse> GetPlatform(G_ScanRequest request, ServerCallContext context)
        {
            var dbGetPlatformRequest = new DbGetPlatformRequest()
            {
                PolicyItemName = request.PolicyItemName,
                PolicyItemVersion = request.PolicyItemVersion
            };
            var platformResponse = dbServiceHandler.DbGetPlatform(dbGetPlatformRequest);
            switch (platformResponse.Platform.ToUpper())
            {
                case "WIN":
                    Console.WriteLine($"Sending scan {platformResponse} request...");
                    G_ScanWinRequest scanwin = new G_ScanWinRequest()
                    {
                        ScanRequest = request
                    };
                    var scanningWin = ScanWinHandler.ScanWinProcess(scanwin);

                    Console.WriteLine(scanningWin);
                    break;
                case "LINUX":
                    break;
            }

            return Task.FromResult(new G_PlatformRequestResponse()
            {
                Response = $"{platformResponse}"
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
        public void LoadDB1()
        {
            var group = UoW.Query<Group>().FirstOrDefault(p_o => p_o.Name == "MyGroup");
            //
            var endpoint = new Endpoint(UoW);
            endpoint.Address = "10.1.5.227";
            endpoint.Name = "MyEndpoint1";
            //
            group.Children.Add(endpoint);
            //
            var runTimeCollection = UoW.Query<RuntimeCollection>().FirstOrDefault(p_o => p_o.Oid == 1);
            endpoint.RuntimeCollections.Add(runTimeCollection);
            
            UoW.CommitChanges();
        }
    }
}
