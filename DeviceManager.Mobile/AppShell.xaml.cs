using DeviceManager.Mobile.Views;

namespace DeviceManager.Mobile;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        RegisterRoutes();
    }

    private void RegisterRoutes()
    {
        Routing.RegisterRoute(nameof(DeviceItemPage), typeof(DeviceItemPage));
        Routing.RegisterRoute(nameof(SyncPage), typeof(SyncPage));
    }
}
