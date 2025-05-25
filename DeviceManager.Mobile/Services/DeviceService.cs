using DeviceManager.Mobile.Interfaces;
using DeviceManager.Mobile.Models;

namespace DeviceManager.Mobile.Services
{
    public class DeviceService : IDeviceService
    {
        IRestService _restService;

        public DeviceService(IRestService restService)
        {
            _restService = restService;
        }

        public Task<List<Dispositivo>> GetTasksAsync()
        {
            return _restService.RefreshDataAsync();
        }

        public Task SaveTaskAsync(Dispositivo item, bool isNewItem = false)
        {
            return _restService.SaveDeviceItemAsync(item, isNewItem);
        }

        public Task DeleteTaskAsync(Dispositivo item)
        {
            return _restService.DeleteDeviceItemAsync(item.ID);
        }
    }
}
