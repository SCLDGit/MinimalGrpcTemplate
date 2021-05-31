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
            //Console.WriteLine($"DB DbGetRegistryEntry method was invoked with: {request}");
            //var policy = UoW.Query<PolicyVersionItem>().FirstOrDefault(p_o =>
            //    p_o.Parent.Name == request.PolicyItemName && p_o.Version == request.PolicyItemVersion);
            //var children = new RepeatedField<DbRegistryEntry>();

            //if (policy != null)
            //{
            //    foreach (var control in policy.Controls)
            //    {
            //        foreach (var controlPart in control.ControlParts)
            //        {
            //            if (controlPart is not RegistryEntryItem registryItem) continue;
            //            var dbRegistryEntry = new DbRegistryEntry()
            //            {
            //                BaselineComplianceValue = string.Empty,
            //                CustomComplianceValue = string.Empty,
            //                UseCustomComplianceValue = string.Empty,
            //                ShouldBeRemoved = string.Empty,
            //                RegistryKeyRoot = registryItem.RegistryKeyRoot,
            //                RegistrySubKey = registryItem.RegistrySubKey,
            //                RegistryValueName = registryItem.RegistryValueName,
            //                RegistryValueKind = registryItem.RegistryValueKind
            //            };
            //            children.Add(dbRegistryEntry);
            //        }
            //    }
            //}

            //return Task.FromResult(new DbGetRegistryEntryResponse()
            //{
            //    Children = { children}
            //});
            return null;
        }

        public override Task<DbGetPlatformResponse> DbGetPlatform(DbGetPlatformRequest request, ServerCallContext context)
        {
            Console.WriteLine($"DB GetPlatform method was invoked with: {request}");
            var policy = UoW.Query<PolicyVersionItem>().FirstOrDefault(p_o =>
                p_o.Parent.Name == request.PolicyItemName && p_o.Version == request.PolicyItemVersion);

            var result = "Policy not found";
            if (policy != null)
            {
                var os = (enmSystemTargetType)policy.Parent.SystemTargetType;
                
                if (os.HasFlag(enmSystemTargetType.WINDOWS))
                {
                    result = "WIN";
                }
                else if (os.HasFlag(enmSystemTargetType.LINUX))
                {
                    result = "LINUX";
                }
            }

            return Task.FromResult(new DbGetPlatformResponse()
            {
                Platform = $"{result}"
            });
        }

    }
}
