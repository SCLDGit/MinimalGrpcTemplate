using MinimalGrpcTemplate.Server.Global.IO.Directories;

namespace MinimalGrpcTemplate.Server.Global.IO.Files;

internal static class ApplicationFiles
{
    internal static string LogFile => Path.Combine(ApplicationDirectories.LogsDirectory, "activity.log");
    
    internal static string SocketFile => Path.Combine(ApplicationDirectories.SocketDirectory, "microservice.sock");
}