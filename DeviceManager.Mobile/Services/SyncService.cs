using DeviceManager.Mobile.Interfaces;
using DeviceManager.Mobile.Models;
using DeviceManager.Mobile.Repositories;
using DeviceManager.Mobile.ViewModels;
using System.Diagnostics;

namespace DeviceManager.Mobile.Services
{
    public class SyncService : ISyncService
    {
        private readonly IDeviceRepository _deviceRepository;
        private readonly IRestService _restService;

        public SyncService(IDeviceRepository deviceRepository, IRestService restService)
        {
            _deviceRepository = deviceRepository;
            _restService = restService;
        }

        public async Task<bool> SyncAsync(IProgress<SyncProgressInfo> progress = null)
        {
            try
            {
                progress?.Report(new SyncProgressInfo { Message = "Iniciando sincronização..." });
                
                var pendingChanges = await _deviceRepository.GetUnsynchronizedAsync();
                
                if (!pendingChanges.Any())
                {
                    progress?.Report(new SyncProgressInfo { Message = "Não há alterações pendentes para sincronizar." });
                    return true;
                }

                var totalItems = pendingChanges.Count;
                progress?.Report(new SyncProgressInfo 
                { 
                    Message = $"Iniciando sincronização de {totalItems} itens...",
                    TotalItems = totalItems,
                    ProcessedItems = 0
                });

                try
                {
                    var syncedDevices = await _restService.SyncAllDevicesAsync(pendingChanges);
                    
                    progress?.Report(new SyncProgressInfo 
                    { 
                        Message = "Sincronização concluída com sucesso!",
                        TotalItems = totalItems,
                        ProcessedItems = totalItems
                    });
                    
                    return true;
                }
                catch (Exception ex)
                {
                    progress?.Report(new SyncProgressInfo 
                    { 
                        Message = $"Erro durante a sincronização: {ex.Message}",
                        HasConflict = true,
                        ConflictDetails = ex.Message
                    });
                    return false;
                }
            }
            catch (Exception ex)
            {
                progress?.Report(new SyncProgressInfo 
                { 
                    Message = $"Erro durante a sincronização: {ex.Message}",
                    HasConflict = true,
                    ConflictDetails = ex.Message
                });
                return false;
            }
        }

        public async Task<List<Dispositivo>> GetPendingChangesAsync()
        {
            return await _deviceRepository.GetUnsynchronizedAsync();
        }

        public async Task<bool> HasPendingChangesAsync()
        {
            var pendingChanges = await _deviceRepository.GetUnsynchronizedAsync();
            return pendingChanges.Any();
        }

        public async Task<bool> ClearAndSyncAsync(IProgress<SyncProgressInfo> progress = null)
        {
            try
            {
                progress?.Report(new SyncProgressInfo { Message = "Limpando repositório local..." });
                
                // Limpa todo o repositório local
                await _deviceRepository.ClearAllDataAsync();
                
                progress?.Report(new SyncProgressInfo { Message = "Buscando dados da API..." });
                
                // Busca todos os dispositivos da API
                var devices = await _restService.RefreshDataAsync();
                
                progress?.Report(new SyncProgressInfo { Message = $"Salvando {devices.Count} dispositivos localmente..." });
                
                // Salva os dispositivos no repositório local
                foreach (var device in devices)
                {
                    device.IsSynchronized = true;
                    await _deviceRepository.AddAsync(device);
                }
                
                progress?.Report(new SyncProgressInfo { Message = "Sincronização concluída com sucesso!" });
                return true;
            }
            catch (Exception ex)
            {
                progress?.Report(new SyncProgressInfo 
                { 
                    Message = $"Erro durante a sincronização: {ex.Message}",
                    HasConflict = true,
                    ConflictDetails = ex.Message
                });
                return false;
            }
        }

        public async Task<bool> ClearLocalRepositoryAsync()
        {
            try
            {
                await _deviceRepository.ClearAllDataAsync();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao limpar repositório local: {ex.Message}");
                return false;
            }
        }
    }
} 