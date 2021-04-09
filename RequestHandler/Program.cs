using System;
using Grpc.Core;
using RequestHandlerService;

namespace RequestHandler
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting request handler server...");

            const int port = 30053;

            var server = new Server()
            {
                Services = { Handler.BindService(new Services.HandlerService()) },
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