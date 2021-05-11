using System;
using Grpc.Core;
using PlatformService;

namespace PlatformHandler
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting platform handler server...");

            const int port = 30053;

            var server = new Server()
            {
                Services = { Platform.BindService(new Services.PlatformService()) },
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
