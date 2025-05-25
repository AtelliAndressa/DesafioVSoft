using System.Net.Http.Json;
using DeviceManager.Web.Models;

namespace DeviceManager.Web.Services
{
    public class BackendService
    {
        private readonly HttpClient _httpClient;

        public BackendService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<Dispositivo>> GetDevicesAsync()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<Dispositivo>>("api/device") ?? new List<Dispositivo>();
        }

        public async Task<Dispositivo> GetDeviceByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<Dispositivo>($"api/device/{id}") ?? new Dispositivo();
        }

        public async Task<Dispositivo> CreateDeviceAsync(Dispositivo device)
        {
            var response = await _httpClient.PostAsJsonAsync("api/device", device);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Dispositivo>() ?? new Dispositivo();
        }

        public async Task UpdateDeviceAsync(int id, Dispositivo device)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/device/{id}", device);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteDeviceAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/device/{id}");
            response.EnsureSuccessStatusCode();
        }
    }
} 