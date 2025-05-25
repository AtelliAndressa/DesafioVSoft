using System.Net.Http.Json;
using DeviceManager.Web.Models;

namespace DeviceManager.Web.Services
{
    public class DeviceService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<DeviceService> _logger;

        public DeviceService(HttpClient httpClient, ILogger<DeviceService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<Dispositivo>> GetAllDevicesAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<Dispositivo>>("api/device");
                return response ?? new List<Dispositivo>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all devices");
                throw;
            }
        }

        public async Task<Dispositivo> GetDeviceByIdAsync(string id)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<Dispositivo>($"api/device/{id}");
                if (response == null)
                    throw new Exception($"Device with ID {id} not found");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting device with ID {Id}", id);
                throw;
            }
        }

        public async Task<Dispositivo> CreateDeviceAsync(Dispositivo device)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/device", device);
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadFromJsonAsync<Dispositivo>();
                if (result == null)
                    throw new Exception("Failed to create device");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating device");
                throw;
            }
        }

        public async Task UpdateDeviceAsync(string id, Dispositivo device)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/device/{id}", device);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating device with ID {Id}", id);
                throw;
            }
        }

        public async Task DeleteDeviceAsync(string id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/device/{id}");
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting device with ID {Id}", id);
                throw;
            }
        }
    }
}
