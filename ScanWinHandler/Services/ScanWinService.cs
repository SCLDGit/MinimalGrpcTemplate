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
using RegistryEntryService;
using DatabaseService;
using PlatformService;

namespace ScanWinHandler.Services
{
    public class ScanWinService : ScanWin.ScanWinBase
    {
        public ScanWinService()
        {
            UoW = new UnitOfWork();
            RegistryEntryHandler = new RegistryEntry.RegistryEntryClient(new Channel("localhost:30056", ChannelCredentials.Insecure));
            DbServiceHandler = new DbService.DbServiceClient(new Channel("localhost:30057", ChannelCredentials.Insecure));
        }
        public UnitOfWork UoW { get; }

        private RegistryEntry.RegistryEntryClient RegistryEntryHandler { get; }
        private DbService.DbServiceClient DbServiceHandler { get; }

        public override async Task<G_ScanWinResponse> ScanWinProcess(G_ScanWinRequest request, ServerCallContext context)
        {
            Console.WriteLine($"'ScanWinProcess' was invoked");
            var dbGetRegistryEntryRequest = new DbGetRegistryEntryRequest()
            {
                PolicyItemName = request.PolicyItemName,
                PolicyItemVersion = request.PolicyItemVersion
            };
            var dbGetRegistryEntry = DbServiceHandler.DbGetRegistryEntry(dbGetRegistryEntryRequest);

            var registryEntryRequest = new RegistryEntryRequest()
            {
                Children = { dbGetRegistryEntry.Children}
            };

            var totalControls = 0;
            try
            {
                using var call = RegistryEntryHandler.ScanRegistryEntry(registryEntryRequest);
                while (await call.ResponseStream.MoveNext())
                {
                    var control = call.ResponseStream.Current;
                    Console.WriteLine("Received: " + control);
                    totalControls++;
                }
            }
            catch (RpcException e)
            {
                Console.WriteLine(e.InnerException.Message);
                throw;
            }
            
            return await Task.FromResult(new G_ScanWinResponse()
            {
                Response = $"{totalControls} registry controls scanned."
            });

        }
    }
}
