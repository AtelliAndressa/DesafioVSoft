using DeviceManager.Web.Models;
using DeviceManager.Web.Services;
using Microsoft.AspNetCore.Components;

namespace DeviceManager.Web.Pages
{
    public partial class DeviceList
    {
        [Inject]
        private DeviceService DeviceService { get; set; }

        [Inject]
        private NavigationManager NavigationManager { get; set; }

        private List<Dispositivo> _devices;
        private bool _showDialog;
        private bool _showDeleteDialog;
        private bool _isEdit;
        private Dispositivo _currentDevice;
        private Dispositivo _deviceToDelete;

        protected override async Task OnInitializedAsync()
        {
            await LoadDevices();
        }

        private async Task LoadDevices()
        {
            _devices = await DeviceService.GetAllDevicesAsync();
        }

        private void ShowCreateDialog()
        {
            _isEdit = false;
            _currentDevice = new Dispositivo();
            _showDialog = true;
        }

        private void ShowEditDialog(Dispositivo device)
        {
            _isEdit = true;
            _currentDevice = new Dispositivo
            {
                Id = device.Id,
                Descricao = device.Descricao,
                CodigoReferencia = device.CodigoReferencia,
                DataCriacao = device.DataCriacao,
                DataAtualizacao = device.DataAtualizacao
            };
            _showDialog = true;
        }

        private void ShowDeleteDialog(Dispositivo device)
        {
            _deviceToDelete = device;
            _showDeleteDialog = true;
        }

        private void CloseDialog()
        {
            _showDialog = false;
            _currentDevice = null;
        }

        private void CloseDeleteDialog()
        {
            _showDeleteDialog = false;
            _deviceToDelete = null;
        }

        private async Task HandleValidSubmit()
        {
            try
            {
                if (_isEdit)
                {
                    await DeviceService.UpdateDeviceAsync(_currentDevice.Id, _currentDevice);
                }
                else
                {
                    await DeviceService.CreateDeviceAsync(_currentDevice);
                }

                CloseDialog();
                await LoadDevices();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private async Task DeleteDevice()
        {
            try
            {
                await DeviceService.DeleteDeviceAsync(_deviceToDelete.Id);
                CloseDeleteDialog();
                await LoadDevices();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
} 