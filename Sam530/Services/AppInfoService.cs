namespace Sam530.Services
{
    public class AppInfoService
    {
        public string GetVersion()
        {
            // Puedes obtenerla desde AssemblyInfo
            var version = typeof(AppInfoService).Assembly.GetName().Version;
            return version?.ToString() ?? "Unknown";
        }
    }
}
