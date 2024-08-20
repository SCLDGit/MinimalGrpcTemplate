using MinimalGrpcTemplate.Server.Global.IO.Directories;

namespace MinimalGrpcTemplate.Server.Global.IO.Files;

internal static class ApplicationFiles
{
    internal static string LogsFilePath => Path.Combine(ApplicationDirectories.LogsDataPath, "activity.log");
    
    internal static string SocketFile => Path.Combine(ApplicationDirectories.SocketDirectory, "microservice.sock");
}