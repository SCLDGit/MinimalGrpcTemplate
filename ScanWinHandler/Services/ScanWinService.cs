using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Xpo;
using Grpc.Core;
using ScanWinService;
using OrchestratorService;
using SteelCloud.Moonshot.Db;

namespace ScanWinHandler.Services
{
    class ScanWinService : ScanWin.ScanWinBase
    {
        //public Orchestrator.OrchestratorClient OrchestratorClient { get; } = new (new Channel("localhost:30052", ChannelCredentials.Insecure));

        public ScanWinService()
        {
            UoW = new UnitOfWork();
        }
        public UnitOfWork UoW { get; }
        public override Task<G_ScanWinResponse> ScanWinProcess(G_ScanWinRequest request, ServerCallContext context)
        {
            var test = request.Scanrequest.PolicyItemName;
            var policy = UoW.Query<PolicyVersionItem>().FirstOrDefault(p_o =>
                p_o.Parent.Name == request.Scanrequest.PolicyItemName && p_o.Version == request.Scanrequest.PolicyItenVersion);
            var registryControlCount = 0;
            foreach (var control in policy.Controls)
            {
                foreach (var controlPart in control.ControlParts)
                {
                    switch (controlPart)
                    {
                        case RegistryEntryItem registryItem:
                            registryControlCount++;
                            break;
                        case LocalSecurityPolicyEntryItem localSecurityPolicyEntryItem:
                            break;

                    }
                }
                
            }

            return Task.FromResult(new G_ScanWinResponse()
            {
                Response = $"Windows scan {registryControlCount} registry controls, finished successfully"
            });
        }
    }
}
