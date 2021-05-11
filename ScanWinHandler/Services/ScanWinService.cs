using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
using ScanWinService;
using OrchestratorService;

namespace ScanWinHandler.Services
{
    class ScanWinService : ScanWin.ScanWinBase
    {
        //public Orchestrator.OrchestratorClient OrchestratorClient { get; } = new (new Channel("localhost:30052", ChannelCredentials.Insecure));

        public override Task<G_ScanWinResponse> ScanWinProcess(G_ScanWinRequest request, ServerCallContext context)
        {
            var test = request.Scanrequest.PolicyItemName;
            return Task.FromResult(new G_ScanWinResponse()
            {
                Response = "Windows scan finished successfully"
            });
        }
    }
}
