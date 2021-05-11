using System;
using Grpc.Core;
using ScanWinService;

namespace ScanWinHandler
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting win scanner handler server...");
            const int port = 30054;

            var server = new Server()
            {
                Services = { ScanWin.BindService(new Services.ScanWinService())},
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
