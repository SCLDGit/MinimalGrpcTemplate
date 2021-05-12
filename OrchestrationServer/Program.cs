using System;
using System.IO;
using Grpc.Core;
using OrchestratorService;

namespace OrchestrationServer
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine("Starting orchestration server...");

            const int port = 30052;

            var server = new Server()
            {
                Services = { Orchestrator.BindService(new Services.OrchestratorService()) },
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