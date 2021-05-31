using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Xpo;
using Grpc.Core;
using RegistryEntryService;
using SteelCloud.Moonshot.Db;
using SteelCloud.Windows.PowerShellEngine;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using SteelCloud.CommonEnumerations;
using SteelCloud.Windows.PowerShellEngine.Common;
using SteelCloud.Windows.PowerShellEngine.JobConfiguration;
using SteelCloud.Windows.PowerShellEngine.Utilities;


namespace RegistryEntryHandler.Services
{
    public class RegistryEntryService : RegistryEntry.RegistryEntryBase
    {
        public RegistryEntryService()
        {
            UoW = new UnitOfWork();
        }
        public UnitOfWork UoW { get; }
        public virtual Runspace PowerShellRunSpace { get; set; }

        public override async Task ScanRegistryEntry(RegistryEntryRequest request, IServerStreamWriter<RegistryEntryResponse> responseStream, ServerCallContext context)        
        {
            Console.WriteLine($"'ScanRegistryEntry' was invoked with: {request}");

            var endpoint = UoW.Query<Endpoint>().FirstOrDefault(p_o => p_o.Address == request.ScanRequest.EndPointAddress);
            var policy = UoW.Query<PolicyVersionItem>().FirstOrDefault(p_o =>
                p_o.Parent.Name == request.ScanRequest.PolicyItemName && p_o.Version == request.ScanRequest.PolicyItemVersion);
            if (endpoint != null)
            {
                var preparingRunspace = PrepareRunspace(endpoint, request.ScanRequest.EndPointAddress);
                if (string.IsNullOrEmpty(preparingRunspace))
                {
                    var controlParts = new List<IControlPartType>();
                    foreach (var control in policy.Controls)
                    {
                        controlParts.AddRange(control.ControlParts.OfType<RegistryEntryItem>());
                    }
                    var powerShellSession = new PowerShellSession(PowerShellRunSpace, controlParts, enmProcesType.SCAN);
                    powerShellSession.ProcessCommands();
                    int passed = 0;
                    int failed = 0;
                    foreach (var commandResult in powerShellSession.PowerShellCommandsToRun)
                    {
                        var currentValue = GetCurrentValue(commandResult);
                        var complianceValue = GetComplianceValue(commandResult.ControlPart.UseCustomComplianceValue ? commandResult.ControlPart.CustomComplianceValue : commandResult.ControlPart.BaselineComplianceValue);
                        var passFailResult = GetPassFailResult(currentValue, complianceValue);
                        if (passFailResult.ToUpper() == "PASSED")
                        {
                            passed++;
                        }
                        else
                        {
                            failed++;
                        }
                        var registryEntryResponse = new RegistryEntryResponse()
                        {
                            Response = $"Control: {((StigControlItem)commandResult.ControlPart.Parent).GroupId} successfully scanned. Result: {passFailResult}"
                        };

                        await responseStream.WriteAsync(registryEntryResponse);
                        System.Threading.Thread.Sleep(100);
                    }
                    Console.WriteLine($"Total PASS: {passed} FAILED: {failed}");
                }
            }

            //foreach (var registryEntry in request.Children)
                //{
                //    Console.WriteLine($"Scanning {registryEntry.RegistryValueName}");
                //    var registryEntryResponse = new RegistryEntryResponse()
                //    {
                //        Response = $"{registryEntry.RegistryValueName} successfully scanned."
                //    };

                //    await responseStream.WriteAsync(registryEntryResponse);
                //    //System.Threading.Thread.Sleep(200);
                //}
        }

        private string PrepareRunspace(Endpoint p_endpoint, string endpointAddress)
        {
            var domainUserName = IpUtilities.GetDomainAndUserFromUserName(p_endpoint.UserName);
            var job = new JobConfiguration(endpointAddress, new Credentials($@"{domainUserName.Item1}\{domainUserName.Item2}", Credentials.ConvertToSecureString(p_endpoint.UserPassword)));

            PowerShellRunSpace = PowerShellUtilities.GetPowerShellRunSpace(job, endpointAddress);
            if (PowerShellRunSpace != null)
            {
                try
                {
                    PowerShellUtilities.SetUpRemotePowershellConnection(job);
                    try
                    {
                        PowerShellRunSpace.Open();
                    }
                    catch (Exception exc)
                    {
                        return $"Unable to open PowerShell runspace. {exc.Message}";
                    }
                }
                catch (Exception exc)
                {
                    return $"Unable to connect to PowerShell. {exc.Message}";
                }
            }
            return string.Empty;
        }

        private string GetCurrentValue(PowerShellCommand p_commandResult)
        {
            var output = string.Empty;
            if (p_commandResult.CommandOutput.Any())
            {
                foreach (var commandOutput in p_commandResult.CommandOutput.ToList())
                {
                    output = output + commandOutput + ",";
                }
                output = output.TrimEnd(',');
            }
            return output;
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

        private string GetPassFailResult(string p_currentValue, string p_complianceValue)
        {
            return p_currentValue == p_complianceValue ? "PASSED" : "FAILED";
        }
    }
}
