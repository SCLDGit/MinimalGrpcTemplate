namespace MinimalGrpcTemplate.MicroService.Global.IO.Directories;

internal static class ApplicationDirectories
{
    #if DEBUG
    private static string MicroserviceDataDirectory =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                     "MinimalGrpcTemplate", "Microservice", "Debug");
    #else
    private static string MicroserviceDataDirectory =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                     "MinimalGrpcTemplate", "Microservice");
    #endif

    internal static string LogsDirectory => Path.Combine(MicroserviceDataDirectory, "Logs");
    internal static string SocketDirectory => Path.Combine(MicroserviceDataDirectory, "Sockets");

    internal static void CreateRequiredDirectories()
    {
        // Clean up existing socket if necessary. - Comment by Matt Heimlich on 08/20/2024 @ 12:37:11
        if ( Directory.Exists(SocketDirectory) )
        {
            Directory.Delete(SocketDirectory, true);
        }
        
        // Logs data path is automatically created by ILogger. - Comment by Matt Heimlich on 07/02/2024 @ 10:17:49
        Directory.CreateDirectory(MicroserviceDataDirectory);
        Directory.CreateDirectory(SocketDirectory);
    }
}