using System;
using System.IO;
using DevExpress.Xpo;
using Grpc.Core;
using PlatformService;
using SteelCloud.Moonshot.Db;

namespace PlatformHandler
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
            var uow = new UnitOfWork();


            Console.WriteLine("Starting platform handler server...");

            const int port = 30054;

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
