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
        public override Task<RegistryEntryResponse> ScanRegistryEntry(RegistryEntryRequest request, ServerCallContext context)
        {
            return Task.FromResult(new RegistryEntryResponse()
            {
                Response = $"Scanning registry '{request.RegistryValueName}' finished successfully"
            });

        }
    }
}
