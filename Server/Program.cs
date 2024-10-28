// See https://aka.ms/new-console-template for more information

using System.Runtime.InteropServices;

using Database.User;

using DevExpress.Xpo.DB;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

using MinimalGrpcTemplate.Server.Global.IO.Directories;
using MinimalGrpcTemplate.Server.Global.IO.Files;
using MinimalGrpcTemplate.Server.Global.Utilities.Logging;
using MinimalGrpcTemplate.Server.Services.GrpcServiceImplementations;
using MinimalGrpcTemplate.Server.Services.MicroserviceInteraction;
using MinimalGrpcTemplate.Server.Services.UserManagement;

using Serilog;
using Serilog.Settings;

namespace MinimalGrpcTemplate.Server;

internal static class Program
{
    private static async Task<int> Main(string[] p_args)
    {
        ApplicationDirectories.CreateRequiredDirectories();

        var appBuilder = WebApplication.CreateBuilder(p_args);

        ConfigureLogging(appBuilder);

        ConfigureServices(appBuilder);

        // Needed to run as a hosted service, if desired. - Comment by Matt Heimlich on 08/20/2024 @ 10:00:16
        if ( RuntimeInformation.IsOSPlatform(OSPlatform.Windows) )
        {
            appBuilder.Host.UseWindowsService();
        }
        else if ( RuntimeInformation.IsOSPlatform(OSPlatform.Linux) )
        {
            appBuilder.Host.UseSystemd();
        }

        appBuilder.Services.AddGrpc();
        
        var app = appBuilder.Build();

        MapGrpcEndpointServices(app);

        Log.Information("Starting server...");
        
        await app.RunAsync();
        
        return 0;
    }
    
    private static void ConfigureLogging(WebApplicationBuilder p_appBuilder)
    {
        p_appBuilder.Logging.ClearProviders();

        Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(p_appBuilder.Configuration)
                                              .WriteTo.Console()
                                              .WriteTo.File(ApplicationFiles.LogFile,
                                                            rollingInterval: RollingInterval.Day,
                                                            retainedFileCountLimit: 31,
                                                            fileSizeLimitBytes: 1024 * 1024 * 10,
                                                            rollOnFileSizeLimit: true)
                                              .WriteTo.Debug()
                                              .CreateLogger();
            
        p_appBuilder.Logging.AddSerilog(Log.Logger);
    }
    
    private static void ConfigureServices(WebApplicationBuilder p_appBuilder)
    {
        p_appBuilder.Services.AddMemoryCache();
        
        p_appBuilder.Services.AddSingleton<IConnectionInfoService, MicroserviceConnectionInfoService>();
        p_appBuilder.Services.AddSingleton<SocketConnectionFactoryService>();
        
        p_appBuilder.Services.AddTransient<UserManagementService>();
        
        p_appBuilder.Services.AddXpoDefaultUnitOfWork(false, p_options =>
                                                                 p_options
                                                                     .UseConnectionString(SQLiteConnectionProvider.GetConnectionString("Database.db"))
                                                                     // Remove this line if the database already exists.
                                                                     .UseAutoCreationOption(AutoCreateOption.DatabaseAndSchema) 
                                                                     .UseConnectionPool(false)
                                                                     // Pass all of your persistent object types to this method.
                                                                     .UseEntityTypes(new Type[] { typeof(User) })); 
    }
    
    private static void MapGrpcEndpointServices(WebApplication p_application)
    {
        p_application.MapGrpcService<ConnectivityServiceImplementation>();
        p_application.MapGrpcService<ServerServiceImplementation>();
            
        #pragma warning disable IL3050
        #pragma warning disable IL2026
        p_application.MapGet("/", () => "Communication with the server must be made through a gRPC client.");
        #pragma warning restore IL2026
        #pragma warning restore IL3050
    }
}