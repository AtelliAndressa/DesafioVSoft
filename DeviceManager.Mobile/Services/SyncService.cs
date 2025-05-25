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
                var processedItems = 0;

                progress?.Report(new SyncProgressInfo 
                { 
                    Message = $"Iniciando sincronização de {totalItems} itens...",
                    TotalItems = totalItems,
                    ProcessedItems = processedItems
                });

                foreach (var device in pendingChanges)
                {
                    try
                    {                        
                        if (device.IsDeleted)
                        {
                            progress?.Report(new SyncProgressInfo 
                            { 
                                Message = $"Excluindo dispositivo {device.CodigoReferencia}...",
                                TotalItems = totalItems,
                                ProcessedItems = processedItems
                            });

                            if (!string.IsNullOrEmpty(device.ID))
                            {
                                await _restService.DeleteDeviceAsync(device.ID);
                            }
                            else
                            {
                                Debug.WriteLine($"Dispositivo não tem ID, apenas removendo localmente: {device.CodigoReferencia}");
                            }

                            // Atualiza o status de sincronização do dispositivo excluído
                            device.IsSynchronized = true;
                            await _deviceRepository.UpdateAsync(device);
                        }
                        else
                        {
                            Dispositivo serverDevice = null;
                            if (!string.IsNullOrEmpty(device.ID))
                            {
                                serverDevice = await _restService.GetDeviceAsync(device.ID);
                            }

                            if (serverDevice == null)
                            {
                                progress?.Report(new SyncProgressInfo 
                                { 
                                    Message = $"Criando novo dispositivo {device.CodigoReferencia}...",
                                    TotalItems = totalItems,
                                    ProcessedItems = processedItems
                                });

                                var createdDevice = await _restService.CreateDeviceAsync(device);
                                
                                // Atualiza o dispositivo local com os dados do servidor
                                device.ID = createdDevice.ID;
                                device.Descricao = createdDevice.Descricao;
                                device.CodigoReferencia = createdDevice.CodigoReferencia;
                                device.DataCriacao = createdDevice.DataCriacao;
                                device.DataAtualizacao = createdDevice.DataAtualizacao;
                                device.IsSynchronized = true;
                                
                                await _deviceRepository.UpdateAsync(device);
                            }
                            else
                            {
                                progress?.Report(new SyncProgressInfo 
                                { 
                                    Message = $"Atualizando dispositivo {device.CodigoReferencia}...",
                                    TotalItems = totalItems,
                                    ProcessedItems = processedItems
                                });

                                try
                                {
                                    var updatedDevice = await _restService.UpdateDeviceAsync(device);
                                    
                                    // Atualiza o dispositivo local com os dados do servidor
                                    device.ID = updatedDevice.ID;
                                    device.Descricao = updatedDevice.Descricao;
                                    device.CodigoReferencia = updatedDevice.CodigoReferencia;
                                    device.DataCriacao = updatedDevice.DataCriacao;
                                    device.DataAtualizacao = updatedDevice.DataAtualizacao;
                                    device.IsSynchronized = true;
                                    
                                    await _deviceRepository.UpdateAsync(device);
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine($"Erro ao atualizar dispositivo {device.CodigoReferencia}: {ex.Message}");
                                    throw;
                                }
                            }
                        }

                        processedItems++;
                        progress?.Report(new SyncProgressInfo 
                        { 
                            Message = $"Dispositivo {device.CodigoReferencia} sincronizado com sucesso.",
                            TotalItems = totalItems,
                            ProcessedItems = processedItems
                        });
                    }
                    catch (Exception ex)
                    {                        
                        var errorInfo = new SyncProgressInfo 
                        { 
                            Message = $"Erro ao sincronizar dispositivo {device.CodigoReferencia}: {ex.Message}",
                            TotalItems = totalItems,
                            ProcessedItems = processedItems,
                            HasConflict = true,
                            ConflictDetails = ex.Message
                        };
                        progress?.Report(errorInfo);
                        throw;
                    }
                }

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