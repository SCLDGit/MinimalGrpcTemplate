using System;
using System.IO;
using DevExpress.Xpo;
using Grpc.Core;
using ScanWinService;

namespace ScanWinHandler
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

            Console.WriteLine("Starting win scanner handler server...");
            const int port = 30055;

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
