namespace DeviceManager.Mobile
{
    public static class Constants
    {
        // URL of REST service (Android does not use localhost)
        // Use http cleartext for local deployment. Change to https for production
        public static string LocalhostUrl = DeviceInfo.Platform == DevicePlatform.Android ? "10.0.2.2" : "localhost";
        public static string Scheme = "https";
        public static string Port = "44317";
        public static string RestUrl = $"{Scheme}://{LocalhostUrl}:{Port}/api/device";

        public static void LogApiConfiguration()
        {
            System.Diagnostics.Debug.WriteLine($"API Configuration:");
            System.Diagnostics.Debug.WriteLine($"Platform: {DeviceInfo.Platform}");
            System.Diagnostics.Debug.WriteLine($"LocalhostUrl: {LocalhostUrl}");
        }
    }
}
