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
            Console.WriteLine($"DB DbGetRegistryEntry method was invoked with: {request}");

            var endpoint = UoW.Query<Endpoint>().FirstOrDefault(p_o => p_o.Address == request.ScanRequest.EndPointAddress);
            var policy = UoW.Query<PolicyVersionItem>().FirstOrDefault(p_o =>
                p_o.Parent.Name == request.ScanRequest.PolicyItemName && p_o.Version == request.ScanRequest.PolicyItemVersion);
            var children = new RepeatedField<DbRegistryEntry>();
            
            if (policy != null)
            {
                foreach (var control in policy.Controls)
                {
                    foreach (var controlPart in control.ControlParts)
                    {
                        if (controlPart is not RegistryEntryItem registryItem) continue;
                        var complianceValue = GetComplianceValue(controlPart.UseCustomComplianceValue ? controlPart.CustomComplianceValue : controlPart.BaselineComplianceValue);
                        var dbRegistryEntry = new DbRegistryEntry()
                        {
                            //BaselineComplianceValue = string.Empty,
                            //CustomComplianceValue = string.Empty,
                            //UseCustomComplianceValue = string.Empty,
                            Oid = controlPart.Oid,
                            ComplianceValue = complianceValue,
                            ShouldBeRemoved = registryItem.ShouldBeRemoved,
                            RegistryKeyRoot = registryItem.RegistryKeyRoot,
                            RegistrySubKey = registryItem.RegistrySubKey,
                            RegistryValueName = registryItem.RegistryValueName,
                            RegistryValueKind = registryItem.RegistryValueKind
                        };
                        children.Add(dbRegistryEntry);
                    }
                }
            }

            return Task.FromResult(new DbGetRegistryEntryResponse()
            {
                UserName = endpoint.UserName,
                UserPassword = endpoint.UserPassword,
                Children = { children}
            });
        }

        private string GetComplianceValue(IValueType p_complianceValue)
        {
            var complianceValue = p_complianceValue switch
            {
                ValueEqualsItem valueEqualsItem => valueEqualsItem.Value,
                ValueNotEqualsItem valueNotEqualsItem => valueNotEqualsItem.DefaultRemediationValue,
                ValueBetweenItem valueBetweenItem => valueBetweenItem.DefaultRemediationValue.ToString(),
                ValueMaximumItem valueMaximumItem => valueMaximumItem.DefaultRemediationValue.ToString(),
                ValueMinimumItem valueMinimumItem => valueMinimumItem.DefaultRemediationValue.ToString(),
                ValueAnyItem valueAnyItem => valueAnyItem.DefaultRemediationValue,
                ValueAuditItem valueAuditItem => valueAuditItem.OnSuccess && valueAuditItem.OnFailure ? "3" : valueAuditItem.OnSuccess ? "1" : valueAuditItem.OnFailure ? "2" : "",
                ValueBooleanItem valueBooleanItem => valueBooleanItem.Enabled ? "1" : "0",
                _ => throw new NotImplementedException("Unknown 'Value Type'"),
            };

            return complianceValue;
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
