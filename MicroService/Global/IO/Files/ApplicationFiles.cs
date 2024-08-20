using MinimalGrpcTemplate.MicroService.Global.IO.Directories;

namespace MinimalGrpcTemplate.MicroService.Global.IO.Files;

internal static class ApplicationFiles
{
    internal static string LogFile => Path.Combine(ApplicationDirectories.LogsDirectory, "activity.log");
    internal static string SocketFile => Path.Combine(ApplicationDirectories.SocketDirectory, "microservice.sock");
}