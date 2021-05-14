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
            dbServiceHandler = new DbService.DbServiceClient(new Channel("localhost:30057", ChannelCredentials.Insecure));
        }
        public UnitOfWork UoW { get; }

        private RegistryEntry.RegistryEntryClient RegistryEntryHandler { get; }
        private DbService.DbServiceClient dbServiceHandler { get; }

        public override Task<G_ScanWinResponse> ScanWinProcess(G_ScanWinRequest request, ServerCallContext context)
        {
            var dbGetRegistryEntryRequest = new DbGetRegistryEntryRequest()
            {
                PolicyItemName = request.PolicyItemName,
                PolicyItemVersion = request.PolicyItemVersion
            };
            var dbGetRegistryEntry = dbServiceHandler.DbGetRegistryEntry(dbGetRegistryEntryRequest);

            foreach (var registryEntry in dbGetRegistryEntry.Children)
            {
                var registryEntryRequest = new RegistryEntryRequest()
                {
                    BaselineComplianceValue = string.Empty,
                    CustomComplianceValue = string.Empty,
                    UseCustomComplianceValue = string.Empty,
                    ShouldBeRemoved = string.Empty,
                    RegistryKeyRoot = registryEntry.RegistryKeyRoot,
                    RegistrySubKey = registryEntry.RegistrySubKey,
                    RegistryValueName = registryEntry.RegistryValueName,
                    RegistryValueKind = registryEntry.RegistryValueKind

                };
                Console.WriteLine("Sending registry scanning request...");
                var scanRegistryResponse = RegistryEntryHandler.ScanRegistryEntry(registryEntryRequest);
                Console.WriteLine($"{scanRegistryResponse}");
            }


            return Task.FromResult(new G_ScanWinResponse()
            {
                Response = $"Windows scan {dbGetRegistryEntry} registry controls, finished successfully"
            });
        }
    }
}
