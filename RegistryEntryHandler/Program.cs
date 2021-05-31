using System;
using System.IO;
using Grpc.Core;
using RegistryEntryService;

namespace RegistryEntryHandler
{
    class Program
    {
        static void Main(string[] args)
        {

            var dbLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "fnddata", "debug");

            //Directory.CreateDirectory(dbLocation);

            var dbConnectionHelper = new SteelCloud.Database.SqliteConnectionHelper(dbLocation, "fnddb.db");

            dbConnectionHelper.EstablishDatabaseConnection();

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
