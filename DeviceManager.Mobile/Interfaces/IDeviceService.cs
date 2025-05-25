using DeviceManager.Mobile.Models;

namespace DeviceManager.Mobile.Interfaces
{
    public interface IDeviceService
    {
        Task<List<Dispositivo>> GetTasksAsync();

        Task SaveTaskAsync(Dispositivo item, bool isNewItem);

        Task DeleteTaskAsync(Dispositivo item);
    }
}
