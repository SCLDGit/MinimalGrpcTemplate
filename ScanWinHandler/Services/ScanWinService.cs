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
            //var policy = UoW.Query<PolicyVersionItem>().FirstOrDefault(p_o =>
            //    p_o.Parent.Name == request.PolicyItemName && p_o.Version == request.PolicyItemVersion);
            //var registryControlCount = 0;
            //foreach (var control in policy.Controls)
            //{
            //    foreach (var controlPart in control.ControlParts)
            //    {
            //        switch (controlPart)
            //        {
            //            case RegistryEntryItem registryItem:
            //                registryControlCount++;
            //                Console.WriteLine("Sending registry scan request...");
            //                var registryEntryRequest = new RegistryEntryRequest()
            //                {
            //                    BaselineComplianceValue = string.Empty,
            //                    CustomComplianceValue = string.Empty,
            //                    UseCustomComplianceValue = string.Empty,
            //                    ShouldBeRemoved = string.Empty,
            //                    RegistryKeyRoot = registryItem.RegistryKeyRoot,
            //                    RegistrySubKey = registryItem.RegistrySubKey,
            //                    RegistryValueName = registryItem.RegistryValueName,
            //                    RegistryValueKind = registryItem.RegistryValueKind

            //                };
            //                Console.WriteLine("Sending registry scanning request...");
            //                var scanRegistryResponse = RegistryEntryHandler.ScanRegistryEntry(registryEntryRequest);
            //                Console.WriteLine($"{scanRegistryResponse} - {(control as StigControlItem).GroupId}");
            //                break;
            //            case LocalSecurityPolicyEntryItem localSecurityPolicyEntryItem:
            //                break;

            //        }
            //    }
            //}

            var dbGetRegistryEntryRequest = new DbGetRegistryEntryRequest()
            {
                PolicyItemName = request.PolicyItemName,
                PolicyItemVersion = request.PolicyItemVersion
            };
            var dbGetRegistryEntry = dbServiceHandler.DbGetRegistryEntry(dbGetRegistryEntryRequest);

            
            return Task.FromResult(new G_ScanWinResponse()
            {
                Response = $"Windows scan {dbGetRegistryEntry} registry controls, finished successfully"
            });
        }
    }
}
