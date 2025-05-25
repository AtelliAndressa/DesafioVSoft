using DeviceManager.Mobile.Interfaces;
using DeviceManager.Mobile.Models;
using System.Diagnostics;

namespace DeviceManager.Mobile.Views;

public partial class DeviceListPage : ContentPage
{
    private readonly IDeviceRepository _deviceRepository;
    private readonly ISyncService _syncService;

    public DeviceListPage(IDeviceRepository deviceRepository, ISyncService syncService)
    {
        InitializeComponent();
        _deviceRepository = deviceRepository;
        _syncService = syncService;
    }

    protected async override void OnAppearing()
    {
        base.OnAppearing();
        await RefreshListAsync();
    }

    private async Task RefreshListAsync()
    {
        try
        {
            var devices = await _deviceRepository.GetAllAsync();
            collectionView.ItemsSource = devices;

            // Verifica se existe algum dispositivo não sincronizado
            var hasUnsynchronizedDevices = devices.Any(d => !d.IsSynchronized);
            UpdateSyncButtonState(hasUnsynchronizedDevices);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Erro ao atualizar lista: {ex.Message}");
            await DisplayAlert("Erro", "Não foi possível atualizar a lista de dispositivos", "OK");
        }
    }

    private void UpdateSyncButtonState(bool hasUnsynchronizedDevices)
    {
        syncButton.Text = "Sincronizar";
        syncButton.IsEnabled = hasUnsynchronizedDevices;
    }

    async void OnAddItemClicked(object sender, EventArgs e)
    {
        var navigationParameter = new Dictionary<string, object>
        {
            { nameof(Dispositivo), new Dispositivo { ID = Guid.NewGuid().ToString() } }
        };
        await Shell.Current.GoToAsync(nameof(DeviceItemPage), navigationParameter);
    }

    async void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Dispositivo selectedDevice)
        {
            var navigationParameter = new Dictionary<string, object>
            {
                { nameof(Dispositivo), selectedDevice }
            };
            await Shell.Current.GoToAsync(nameof(DeviceItemPage), navigationParameter);
        }
    }

    async void OnSyncButtonClicked(object sender, EventArgs e)
    {
        try
        {
            var result = await _syncService.SyncAsync();
            if (result)
            {
                await DisplayAlert("Sucesso", "Sincronização concluída com sucesso!", "OK");

            }
            else
            {
                await DisplayAlert("Atenção", "Não existem itens para sincronizar", "OK");
            }

            await RefreshListAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Erro durante sincronização: {ex.Message}");
            await DisplayAlert("Erro", ex.Message, "OK");
            await RefreshListAsync();
        }
    }
}