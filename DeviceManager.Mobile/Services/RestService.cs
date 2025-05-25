using DeviceManager.Mobile.Interfaces;
using DeviceManager.Mobile.Models;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace DeviceManager.Mobile.Services
{
    public class RestService : IRestService
    {
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _serializerOptions;

        public List<Dispositivo> Items { get; private set; }

        public RestService()
        {
            try
            {
                Constants.LogApiConfiguration();

#if DEBUG
                HttpClientHandler insecureHandler = GetInsecureHandler();
                _client = new HttpClient(insecureHandler);
#else
                _client = new HttpClient();
#endif
                _serializerOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                };

                _client.Timeout = TimeSpan.FromSeconds(30);

                _client.DefaultRequestHeaders.Accept.Clear();
                _client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao inicializar RestService: {ex.Message}");
                throw;
            }
        }

        private HttpClientHandler GetInsecureHandler()
        {
            try
            {
                HttpClientHandler handler = new HttpClientHandler();
                handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
                {
                    if (cert != null && cert.Issuer.Equals("CN=localhost"))
                        return true;
                    return errors == System.Net.Security.SslPolicyErrors.None;
                };
                return handler;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao criar handler inseguro: {ex.Message}");
                throw;
            }
        }

        private object MapToApiModel(Dispositivo device)
        {
            var model = new
            {
                Id = device.ID,
                Descricao = device.Descricao,
                CodigoReferencia = device.CodigoReferencia,
                DataCriacao = device.DataCriacao.DateTime,
                DataAtualizacao = device.DataAtualizacao?.DateTime
            };
            
            return model;
        }

        private Dispositivo MapFromApiModel(object apiModel)
        {
            var json = JsonSerializer.Serialize(apiModel, _serializerOptions);
            var device = JsonSerializer.Deserialize<Dispositivo>(json, _serializerOptions);

            return device;
        }

        private async Task<HttpResponseMessage> SendRequestAsync(Func<Task<HttpResponseMessage>> requestFunc, string operation)
        {
            try
            {                
                var response = await requestFunc();
                return response;
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Erro de conexão durante {operation}. Verifique se a API está rodando e acessível em {Constants.RestUrl}", ex);
            }
            catch (TaskCanceledException ex)
            {
                throw new Exception($"Timeout durante {operation}. A requisição demorou muito para responder.", ex);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro inesperado durante {operation}: {ex.Message}");
                throw;
            }
        }

        public async Task<List<Dispositivo>> RefreshDataAsync()
        {
            try
            {
                Uri uri = new Uri(Constants.RestUrl);
                Debug.WriteLine($"GET {uri}");
                
                var response = await SendRequestAsync(() => _client.GetAsync(uri), "GET dispositivos");
                string content = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    var devices = JsonSerializer.Deserialize<List<Dispositivo>>(content, _serializerOptions);
                    foreach (var device in devices)
                    {
                        device.IsSynchronized = true;
                    }
                    return devices;
                }
                else
                {
                    throw new Exception($"Erro ao buscar dispositivos: {response.StatusCode} - {content}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao buscar dispositivos: {ex.Message}");
                throw;
            }
        }

        public async Task<Dispositivo> CreateDeviceAsync(Dispositivo device)
        {
            try
            {
                Uri uri = new Uri(string.Format(Constants.RestUrl, string.Empty));
                var apiModel = MapToApiModel(device);
                string json = JsonSerializer.Serialize(apiModel, _serializerOptions);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _client.PostAsync(uri, content);

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<Dispositivo>(responseContent, _serializerOptions);
                }
                else
                {
                    throw new Exception($"Erro ao criar dispositivo: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao criar dispositivo: {ex.Message}");
                throw;
            }
        }

        public async Task<Dispositivo> UpdateDeviceAsync(Dispositivo device)
        {
            try
            {
                Uri uri = new Uri($"{Constants.RestUrl}/{device.ID}");
                var apiModel = MapToApiModel(device);
                string json = JsonSerializer.Serialize(apiModel, _serializerOptions);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _client.PutAsync(uri, content);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Erro ao atualizar dispositivo: {response.StatusCode}");
                }

                var updatedDevice = await GetDeviceAsync(device.ID);
                if (updatedDevice == null)
                {
                    throw new Exception("Dispositivo não encontrado após atualização");
                }

                return updatedDevice;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao atualizar dispositivo: {ex.Message}");
                throw;
            }
        }

        public async Task DeleteDeviceAsync(string id)
        {
            try
            {
                Uri uri = new Uri(string.Format(Constants.RestUrl, id));
                HttpResponseMessage response = await _client.DeleteAsync(uri);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Erro ao excluir dispositivo: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao excluir dispositivo: {ex.Message}");
                throw;
            }
        }

        public async Task<Dispositivo> GetDeviceAsync(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentException("ID do dispositivo não pode ser nulo ou vazio");
                }

                Uri uri = new Uri($"{Constants.RestUrl}/{id}");

                var response = await SendRequestAsync(() => _client.GetAsync(uri), "GET dispositivo");
                string content = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    var device = MapFromApiModel(JsonSerializer.Deserialize<object>(content, _serializerOptions));
                    device.IsSynchronized = true;
                    return device;
                }
                else
                {
                    Debug.WriteLine($"Erro ao buscar dispositivo: {response.StatusCode}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao buscar dispositivo: {ex.Message}");
                return null;
            }
        }

        public async Task SaveDeviceItemAsync(Dispositivo item, bool isNewItem)
        {
            try
            {
                if (isNewItem)
                {
                    var createdDevice = await CreateDeviceAsync(item);
                    item.ID = createdDevice.ID;
                    item.Descricao = createdDevice.Descricao;
                    item.CodigoReferencia = createdDevice.CodigoReferencia;
                    item.DataCriacao = createdDevice.DataCriacao;
                    item.DataAtualizacao = createdDevice.DataAtualizacao;
                    item.IsSynchronized = true;
                }
                else
                {
                    var updatedDevice = await UpdateDeviceAsync(item);
                    item.Descricao = updatedDevice.Descricao;
                    item.CodigoReferencia = updatedDevice.CodigoReferencia;
                    item.DataAtualizacao = updatedDevice.DataAtualizacao;
                    item.IsSynchronized = true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao salvar dispositivo: {ex.Message}");
                throw;
            }
        }

        public async Task DeleteDeviceItemAsync(string id)
        {
            await DeleteDeviceAsync(id);
        }
    }
}