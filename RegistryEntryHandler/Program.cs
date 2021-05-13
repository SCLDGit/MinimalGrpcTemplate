using System;
using Grpc.Core;
using RegistryEntryService;

namespace RegistryEntryHandler
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting registry entry server...");
            const int port = 30056;

            var server = new Server()
            {
                Services = { RegistryEntry.BindService(new Services.RegistryEntryService())},
                Ports = { new ServerPort("localhost", port, ServerCredentials.Insecure) }
            };

            server.Start();

            Console.WriteLine("Listening...");
            Console.WriteLine("Press any key to stop server...");
            Console.ReadKey(true);

            server.ShutdownAsync().Wait();
        }
    }
}
