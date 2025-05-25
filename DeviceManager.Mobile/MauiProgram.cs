using DeviceManager.Mobile.Converters;
using DeviceManager.Mobile.Interfaces;
using DeviceManager.Mobile.Repositories;
using DeviceManager.Mobile.Services;
using DeviceManager.Mobile.ViewModels;
using DeviceManager.Mobile.Views;
using Microsoft.Extensions.Logging;

namespace DeviceManager.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Configuração de logging
        builder.Logging
            .AddDebug()
            .SetMinimumLevel(LogLevel.Debug);

        // Registro de serviços
        builder.Services.AddSingleton<IRestService, RestService>();
        builder.Services.AddSingleton<IDeviceService, DeviceService>();
        builder.Services.AddSingleton<IDeviceRepository, DeviceRepository>();
        builder.Services.AddSingleton<ISyncService, SyncService>();

        // Registro de ViewModels
        builder.Services.AddSingleton<SyncViewModel>();

        // Registro de Views
        builder.Services.AddSingleton<DeviceListPage>();
        builder.Services.AddTransient<DeviceItemPage>();
        builder.Services.AddSingleton<SyncPage>();

        // Registro de conversores
        builder.Services.AddSingleton<InverseBoolConverter>();

        return builder.Build();
    }
}
