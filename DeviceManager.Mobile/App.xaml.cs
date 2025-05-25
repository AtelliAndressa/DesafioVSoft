namespace DeviceManager.Mobile;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState) // Fixed nullable mismatch
    {
        return new Window(new AppShell());
    }
}