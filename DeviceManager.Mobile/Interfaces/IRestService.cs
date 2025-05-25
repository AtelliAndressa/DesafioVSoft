using DeviceManager.Mobile.Models;

namespace DeviceManager.Mobile.Interfaces
{
    public interface IRestService
    {
        Task<List<Dispositivo>> RefreshDataAsync();

        Task<Dispositivo> CreateDeviceAsync(Dispositivo device);

        Task<Dispositivo> UpdateDeviceAsync(Dispositivo device);

        Task DeleteDeviceAsync(string id);

        Task<Dispositivo> GetDeviceAsync(string id);

        Task<List<Dispositivo>> SyncAllDevicesAsync(List<Dispositivo> devices);
        
        // Métodos legados para compatibilidade
        Task SaveDeviceItemAsync(Dispositivo item, bool isNewItem);

        Task DeleteDeviceItemAsync(string id);
    }
}
