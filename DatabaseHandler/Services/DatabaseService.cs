using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatabaseService;
using DevExpress.Xpo;
using Google.Protobuf.Collections;
using Grpc.Core;
using SteelCloud.CommonEnumerations;
using SteelCloud.Moonshot.Db;

namespace DatabaseHandler.Services
{
    public class DatabaseService : DbService.DbServiceBase
    {
        public DatabaseService()
        {
            UoW = new UnitOfWork();
        }

        public UnitOfWork UoW { get; }

        public override Task<DbGetRegistryEntryResponse> DbGetRegistryEntry(DbGetRegistryEntryRequest request, ServerCallContext context)
        {
            var policy = UoW.Query<PolicyVersionItem>().FirstOrDefault(p_o =>
                p_o.Parent.Name == request.PolicyItemName && p_o.Version == request.PolicyItemVersion);
            var registryControlCount = 0;
            var children = new RepeatedField<DbRegistryEntry>();
            foreach (var control in policy.Controls)
            {
                foreach (var controlPart in control.ControlParts)
                {
                    switch (controlPart)
                    {
                        case RegistryEntryItem registryItem:
                            registryControlCount++;
                            Console.WriteLine("Sending registry scan request...");
                            var dbRegistryEntry = new DbRegistryEntry()
                            {
                                BaselineComplianceValue = string.Empty,
                                CustomComplianceValue = string.Empty,
                                UseCustomComplianceValue = string.Empty,
                                ShouldBeRemoved = string.Empty,
                                RegistryKeyRoot = registryItem.RegistryKeyRoot,
                                RegistrySubKey = registryItem.RegistrySubKey,
                                RegistryValueName = registryItem.RegistryValueName,
                                RegistryValueKind = registryItem.RegistryValueKind
                            };
                            children.Add(dbRegistryEntry);

                           
                            //Console.WriteLine("Sending registry scanning request...");
                            //var scanRegistryResponse = RegistryEntryHandler.ScanRegistryEntry(registryEntryRequest);
                            //Console.WriteLine($"{scanRegistryResponse} - {(control as StigControlItem).GroupId}");
                            break;
                        case LocalSecurityPolicyEntryItem localSecurityPolicyEntryItem:
                            break;

                    }
                }
            }
            return new DbGetRegistryEntryResponse() { Children = { children} };
        }

        public override Task<DbGetPlatformResponse> DbGetPlatform(DbGetPlatformRequest request, ServerCallContext context)
        {
            var policy = UoW.Query<PolicyVersionItem>().FirstOrDefault(p_o =>
                p_o.Parent.Name == request.PolicyItemName && p_o.Version == request.PolicyItemVersion);

            var os = (enmSystemTargetType)policy.Parent.SystemTargetType;
            var result = string.Empty;
            if (os.HasFlag(enmSystemTargetType.WINDOWS))
            {
                result = "WIN";
            }
            else if (os.HasFlag(enmSystemTargetType.LINUX))
            {
                result = "LINUX";
            }

            return Task.FromResult(new DbGetPlatformResponse()
            {
                Platform = $"{result}"
            });
        }
    }
}
