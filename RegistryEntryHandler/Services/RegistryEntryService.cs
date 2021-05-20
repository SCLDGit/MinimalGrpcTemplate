using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
using RegistryEntryService;


namespace RegistryEntryHandler.Services
{
    public class RegistryEntryService : RegistryEntry.RegistryEntryBase
    {
        public override async Task ScanRegistryEntry(RegistryEntryRequest request, IServerStreamWriter<RegistryEntryResponse> responseStream, ServerCallContext context)
        {
            //Console.WriteLine($"'ScanRegistryEntry' was invoked with: {request}");

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
}
