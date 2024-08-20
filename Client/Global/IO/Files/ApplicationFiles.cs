using MinimalGrpcTemplate.Client.Global.IO.Directories;

namespace MinimalGrpcTemplate.Client.Global.IO.Files;

internal static class ApplicationFiles
{
    internal static string LogFile => Path.Combine(ApplicationDirectories.LogsDirectory, "activity.log");
}