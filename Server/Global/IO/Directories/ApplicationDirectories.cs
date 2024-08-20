namespace MinimalGrpcTemplate.Server.Global.IO.Directories;

internal static class ApplicationDirectories
{
    #if DEBUG
    private static string ServerDataDirectory =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                     "MinimalGrpcTemplate", "Client", "Debug");
    
    private static string MicroserviceDataDirectory =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                     "MinimalGrpcTemplate", "Microservice", "Debug");
    #else
    private static string ServerDataDirectory =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                     "MinimalGrpcTemplate", "Client");
    
    private static string MicroserviceDataDirectory =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                     "MinimalGrpcTemplate", "Microservice");
    #endif

    internal static string LogsDataPath    => Path.Combine(ServerDataDirectory, "Logs");
    internal static string SocketDirectory => Path.Combine(MicroserviceDataDirectory, "Sockets");

    internal static void CreateRequiredDirectories()
    {
        // Logs data path is automatically created by ILogger. - Comment by Matt Heimlich on 07/02/2024 @ 10:17:49
        Directory.CreateDirectory(ServerDataDirectory);
        Directory.CreateDirectory(SocketDirectory);
    }
}