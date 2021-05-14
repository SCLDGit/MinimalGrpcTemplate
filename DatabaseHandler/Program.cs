using System;
using System.IO;
using DatabaseService;
using Grpc.Core;

namespace DatabaseHandler
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


            Console.WriteLine("Starting database handler server...");
            const int port = 30057;

            var server = new Server()
            {
                Services = { DbService.BindService(new Services.DatabaseService())},
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
