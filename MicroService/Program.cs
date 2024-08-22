// See https://aka.ms/new-console-template for more information

using System.Runtime.InteropServices;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using MinimalGrpcTemplate.MicroService.Global.IO.Directories;
using MinimalGrpcTemplate.MicroService.Global.IO.Files;
using MinimalGrpcTemplate.MicroService.Global.Utilities.Logging;
using MinimalGrpcTemplate.MicroService.Services.GrpcServiceImplementations;

using Serilog;

namespace MinimalGrpcTemplate.MicroService;

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
        
        appBuilder.WebHost.UseKestrel();

        appBuilder.WebHost.ConfigureKestrel(p_options =>
                                            {
                                                p_options.ListenUnixSocket(ApplicationFiles.SocketFile);
                                             
                                                p_options.ConfigureEndpointDefaults(p_listenOptions =>
                                                                                    {
                                                                                        p_listenOptions.Protocols = HttpProtocols.Http2;
                                                                                    });
                                            });
        
        var app = appBuilder.Build();

        MapGrpcEndpointServices(app);

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
    }
    
    private static void MapGrpcEndpointServices(WebApplication p_application)
    {
        p_application.MapGrpcService<ConnectivityServiceImplementation>();
        p_application.MapGrpcService<MicroserviceServiceImplementation>();
            
        #pragma warning disable IL3050
        #pragma warning disable IL2026
        p_application.MapGet("/", () => "Communication with the server must be made through a gRPC client.");
        #pragma warning restore IL2026
        #pragma warning restore IL3050
    }
}