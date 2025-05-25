using DeviceManager.Web.Models;
using DeviceManager.Web.Services;
using Microsoft.AspNetCore.Components;

namespace DeviceManager.Web.Pages
{
    public partial class Edit
    {
        [Parameter]
        public string Id { get; set; }

        [Inject]
        private DeviceService DeviceService { get; set; }

        [Inject]
        private NavigationManager NavigationManager { get; set; }

        private Dispositivo device = new();

        protected override async Task OnParametersSetAsync()
        {
            if (!string.IsNullOrEmpty(Id))
            {
                device = await DeviceService.GetDeviceByIdAsync(Id);
            }
        }

        private async Task HandleSubmit()
        {
            if (string.IsNullOrEmpty(Id))
            {
                await DeviceService.CreateDeviceAsync(device);
            }
            else
            {
                await DeviceService.UpdateDeviceAsync(device.Id, device);
            }
            NavigationManager.NavigateTo("/");
        }

        private void Cancel()
        {
            NavigationManager.NavigateTo("/");
        }
    }
} 