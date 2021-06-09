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
            var children = new RepeatedField<MRegistryEntry>();
            
            if (policy != null)
            {
                foreach (var control in policy.Controls)
                {
                    switch (control)
                    {
                        case StigControlItem stigControlItem:
                            if (stigControlItem.IsDocumentation) continue;
                            var MStigControlItem = new MStigControlItem()
                            {
                                GroupId = stigControlItem.GroupId,
                                GroupTitle = stigControlItem.GroupTitle,
                                RuleId = stigControlItem.RuleId,
                                RuleTitle = stigControlItem.RuleTitle,
                                RuleSeverity = stigControlItem.RuleSeverity,
                                RuleWeight = stigControlItem.RuleWeight,
                                RuleVersion = stigControlItem.RuleVersion,
                                VulnerabilityDiscussion = stigControlItem.VulnerabilityDiscussion,
                                CheckContent = stigControlItem.CheckContent,
                                FixId = stigControlItem.FixId,
                                FixText = stigControlItem.FixText,
                                Cci = stigControlItem.CCI,

                                MIPolicyControlType = new MIPolicyControlType()
                                {
                                    Comments = stigControlItem.Comments,
                                    IsReadOnly = stigControlItem.IsReadOnly,
                                    IsDocumentation = stigControlItem.IsDocumentation,
                                }
                            };
                            var mRegistryEntryItems = new RepeatedField<MRegistryEntryItem>();
                            foreach (var controlPart in control.ControlParts)
                            {
                                if (controlPart is not RegistryEntryItem registryItem) continue;
                                var isRegMultiSz = registryItem.RegistryValueKind.ToUpper() == "REG_MULTI_SZ";
                                var mRegistryEntryItem = new MRegistryEntryItem()
                                {
                                    RegistryKeyRoot = registryItem.RegistryKeyRoot,
                                    RegistrySubKey = registryItem.RegistrySubKey,
                                    RegistryValueName = registryItem.RegistryValueName,
                                    RegistryValueKind = registryItem.RegistryValueKind,

                                    MIControlPartType = new MIControlPartType()
                                    {
                                        BaselineComplianceValue = GetValueFromValueType(registryItem.BaselineComplianceValue, isRegMultiSz),
                                        CustomComplianceValue = GetValueFromValueType(registryItem.CustomComplianceValue, isRegMultiSz),
                                        UseCustomComplianceValue = registryItem.UseCustomComplianceValue,
                                        ShouldBeRemoved = registryItem.ShouldBeRemoved,
                                        Comments = registryItem.Comments,
                                        AcceptedNonCompliance = registryItem.AcceptedNonCompliance,
                                        //AcceptedNonComplianceExpirationDate = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(registryItem.AcceptedNonComplianceExpirationDate),
                                        LockRemediation = registryItem.LockRemediation,
                                        ApplyForAllUsers = registryItem.ApplyForAllUsers
                                    }

                                };
                                mRegistryEntryItems.Add(mRegistryEntryItem);
                            }
                            if(mRegistryEntryItems.Count > 0)
                            {
                                MRegistryEntry mRegistryEntry = new MRegistryEntry()
                                {
                                    MStigControlItem = MStigControlItem,
                                    MRegistryEntryItems = { mRegistryEntryItems },
                                };
                                children.Add(mRegistryEntry);
                            }
                            break;
                        case CisControlItem cisControlItem:
                            break;
                        default:
                            break;
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

        //private string GetComplianceValue(IValueType p_complianceValue)
        //{
        //    var complianceValue = p_complianceValue switch
        //    {
        //        ValueEqualsItem valueEqualsItem => valueEqualsItem.Value,
        //        ValueNotEqualsItem valueNotEqualsItem => valueNotEqualsItem.DefaultRemediationValue,
        //        ValueBetweenItem valueBetweenItem => valueBetweenItem.DefaultRemediationValue.ToString(),
        //        ValueMaximumItem valueMaximumItem => valueMaximumItem.DefaultRemediationValue.ToString(),
        //        ValueMinimumItem valueMinimumItem => valueMinimumItem.DefaultRemediationValue.ToString(),
        //        ValueAnyItem valueAnyItem => valueAnyItem.DefaultRemediationValue,
        //        ValueAuditItem valueAuditItem => valueAuditItem.OnSuccess && valueAuditItem.OnFailure ? "3" : valueAuditItem.OnSuccess ? "1" : valueAuditItem.OnFailure ? "2" : "",
        //        ValueBooleanItem valueBooleanItem => valueBooleanItem.Enabled ? "1" : "0",
        //        _ => throw new NotImplementedException("Unknown 'Value Type'"),
        //    };

        //    return complianceValue;
        //}
        private static string GetValueFromValueType(IValueType p_valueType, bool p_isRegMultiSz)
        {
            return p_valueType switch
            {
                ValueEqualsItem valueEqualsItem => valueEqualsItem.Value,
                ValueNotEqualsItem valueNotEqualsItem => $"[!{valueNotEqualsItem.Value}],{valueNotEqualsItem.DefaultRemediationValue}",
                ValueBetweenItem valueBetweenItem => $"[{valueBetweenItem.Value1}...{valueBetweenItem.Value2}],{valueBetweenItem.DefaultRemediationValue}",
                ValueMaximumItem valueMaximumItem => $"[Max...{valueMaximumItem.Value}],{valueMaximumItem.DefaultRemediationValue}",
                ValueMinimumItem valueMinimumItem => $"[Min...{valueMinimumItem.Value}],{valueMinimumItem.DefaultRemediationValue}",
                ValueAnyItem valueAnyItem => p_isRegMultiSz ? $"{valueAnyItem.Value.Replace("~|~", ",")}" : $"[{valueAnyItem.Value.Replace("~|~", "|")}],{valueAnyItem.DefaultRemediationValue}",
                ValueAuditItem valueAuditItem => valueAuditItem.OnSuccess && valueAuditItem.OnFailure ? "3" : valueAuditItem.OnSuccess ? "1" : valueAuditItem.OnFailure ? "2" : "",
                ValueBooleanItem valueBooleanItem => valueBooleanItem.Enabled ? "1" : "0",
                _ => throw new NotImplementedException("Unknown 'Value Type'"),
            };
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
