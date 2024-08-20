namespace MinimalGrpcTemplate.Client.Global.IO.Directories;

internal static class ApplicationDirectories
{
    #if DEBUG
    private static string ClientDataDirectory =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                     "MinimalGrpcTemplate", "Client", "Debug");
    #else
    private static string ClientDataDirectory =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                     "MinimalGrpcTemplate", "Client");
    #endif

    internal static string LogsDirectory => Path.Combine(ClientDataDirectory, "Logs");

    internal static void CreateRequiredDirectories()
    {
        // Logs data path is automatically created by ILogger. - Comment by Matt Heimlich on 07/02/2024 @ 10:17:49
        Directory.CreateDirectory(ClientDataDirectory);
    }
}