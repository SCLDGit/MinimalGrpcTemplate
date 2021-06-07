using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
using RegistryEntryService;
using SteelCloud.Windows.PowerShellEngine.JobConfiguration;
using SteelCloud.Windows.PowerShellEngine.Utilities;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace RegistryEntryHandler.Services
{
    
    public class RegistryEntryService : RegistryEntry.RegistryEntryBase
    {
        public virtual Runspace PowerShellRunSpace { get; set; }
        public override async Task ScanRegistryEntry(RegistryEntryRequest request, IServerStreamWriter<RegistryEntryResponse> responseStream, ServerCallContext context)
        {
            //Console.WriteLine($"'ScanRegistryEntry' was invoked with: {request}");

            var preparingRunspace = PrepareRunspace(request.UserName, request.UserPassword, request.EndpointAddress);
            if (string.IsNullOrEmpty(preparingRunspace))
            {
                foreach (var registryEntry in request.Children)
                {
                    Console.WriteLine($"Scanning {registryEntry.RegistryValueName}");





                    var registryEntryResponse = new RegistryEntryResponse()
                    {
                        Response = $"{registryEntry.RegistryValueName} successfully scanned."
                    };

                    await responseStream.WriteAsync(registryEntryResponse);
                    //System.Threading.Thread.Sleep(200);
                }
            }
            
        }

        private string PrepareRunspace(string p_userName, string p_userPassword, string endpointAddress)
        {
            var domainUserName = IpUtilities.GetDomainAndUserFromUserName(p_userName);
            var job = new JobConfiguration(endpointAddress, new Credentials($@"{domainUserName.Item1}\{domainUserName.Item2}", Credentials.ConvertToSecureString(p_userPassword)));

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
    }
}
