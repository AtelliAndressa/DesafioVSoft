using DeviceManager.Mobile.Interfaces;
using DeviceManager.Mobile.Models;
using Realms;
using System.Linq.Expressions;
using System.Diagnostics;

namespace DeviceManager.Mobile.Repositories
{
    public class DeviceRepository : IDeviceRepository
    {
        private readonly Realm _realm;

        public DeviceRepository()
        {
            var config = new RealmConfiguration
            {
                SchemaVersion = 1,
                ShouldDeleteIfMigrationNeeded = true
            };

            //// ⚠️ Limpa o banco local toda vez que esse repositório for criado
            //Realm.DeleteRealm(config);

            _realm = Realm.GetInstance(config);
        }

        public void DeleteRealmFile()
        {
            try
            {
                var config = new RealmConfiguration
                {
                    SchemaVersion = 1
                };

                Realm.DeleteRealm(config);
                Debug.WriteLine("Arquivo Realm local deletado com sucesso.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao deletar arquivo Realm: {ex.Message}");
                throw;
            }
        }


        public Task<List<Dispositivo>> GetAllAsync()
        {
            return Task.FromResult(_realm.All<Dispositivo>()
                .Where(d => !d.IsDeleted)
                .OrderByDescending(d => d.DataAtualizacao)
                .ToList());
        }

        public async Task<Dispositivo> GetByIdAsync(string id)
        {
            return await Task.FromResult(_realm.Find<Dispositivo>(id));
        }

        public async Task<Dispositivo> GetByCodigoReferenciaAsync(string codigoReferencia)
        {
            return await Task.FromResult(_realm.All<Dispositivo>()
                .FirstOrDefault(d => d.CodigoReferencia == codigoReferencia && !d.IsDeleted));
        }

        public async Task AddAsync(Dispositivo dispositivo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dispositivo.CodigoReferencia))
                {
                    throw new InvalidOperationException("O código de referência é obrigatório");
                }

                if (await CodigoReferenciaExisteAsync(dispositivo.CodigoReferencia))
                {
                    throw new InvalidOperationException($"Já existe um dispositivo com o código de referência {dispositivo.CodigoReferencia}");
                }

                await _realm.WriteAsync(() =>
                {
                    dispositivo.DataCriacao = DateTimeOffset.Now;
                    dispositivo.DataAtualizacao = DateTimeOffset.Now;
                    dispositivo.IsSynchronized = false;
                    dispositivo.IsDeleted = false;
                    _realm.Add(dispositivo);
                });

                Debug.WriteLine($"Dispositivo adicionado: {dispositivo.CodigoReferencia}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao adicionar dispositivo: {ex.Message}");
                throw;
            }
        }

        public async Task UpdateAsync(Dispositivo dispositivo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dispositivo.CodigoReferencia))
                {
                    throw new InvalidOperationException("O código de referência é obrigatório");
                }

                var existingDevice = await GetByIdAsync(dispositivo.ID);
                if (existingDevice == null)
                {
                    throw new InvalidOperationException($"Dispositivo com ID {dispositivo.ID} não encontrado");
                }

                if (existingDevice.CodigoReferencia != dispositivo.CodigoReferencia &&
                    await CodigoReferenciaExisteAsync(dispositivo.CodigoReferencia))
                {
                    throw new InvalidOperationException($"Já existe um dispositivo com o código de referência {dispositivo.CodigoReferencia}");
                }

                Debug.WriteLine($"Atualizando dispositivo localmente: {dispositivo.CodigoReferencia}");
                Debug.WriteLine($"ID: {dispositivo.ID}");
                Debug.WriteLine($"IsSynchronized: {dispositivo.IsSynchronized}");
                Debug.WriteLine($"DataAtualizacao: {dispositivo.DataAtualizacao}");

                await _realm.WriteAsync(() =>
                {
                    dispositivo.DataAtualizacao = DateTimeOffset.Now;
                    _realm.Add(dispositivo, update: true);
                });

                Debug.WriteLine($"Dispositivo atualizado localmente com sucesso: {dispositivo.CodigoReferencia}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao atualizar dispositivo localmente: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task DeleteAsync(Dispositivo dispositivo)
        {
            try
            {
                var existingDevice = await GetByIdAsync(dispositivo.ID);
                if (existingDevice == null)
                {
                    throw new InvalidOperationException($"Dispositivo com ID {dispositivo.ID} não encontrado");
                }

                await _realm.WriteAsync(() =>
                {
                    dispositivo.IsDeleted = true;
                    dispositivo.IsSynchronized = false;
                    dispositivo.DataAtualizacao = DateTimeOffset.Now;
                    _realm.Add(dispositivo, update: true);
                });

                Debug.WriteLine($"Dispositivo marcado como excluído: {dispositivo.CodigoReferencia}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao excluir dispositivo: {ex.Message}");
                throw;
            }
        }

        public Task<List<Dispositivo>> GetUnsynchronizedAsync()
        {
            return Task.FromResult(_realm.All<Dispositivo>()
                .Where(d => !d.IsSynchronized)
                .OrderBy(d => d.DataAtualizacao)
                .ToList());
        }

        public Task<bool> CodigoReferenciaExisteAsync(string codigoReferencia)
        {
            return Task.FromResult(_realm.All<Dispositivo>()
                .Any(d => d.CodigoReferencia == codigoReferencia && !d.IsDeleted));
        }

        public async Task MarcarComoSincronizadoAsync(string id)
        {
            var dispositivo = await GetByIdAsync(id);
            if (dispositivo == null)
                throw new InvalidOperationException($"Dispositivo com ID {id} não encontrado");

            await _realm.WriteAsync(() =>
            {
                dispositivo.IsSynchronized = true;
                dispositivo.DataAtualizacao = DateTimeOffset.Now;
            });

            Debug.WriteLine($"Dispositivo marcado como sincronizado: {dispositivo.CodigoReferencia}");
        }

        public Task<int> GetPendingChangesCountAsync()
        {
            return Task.FromResult(_realm.All<Dispositivo>()
                .Count(d => !d.IsSynchronized));
        }

        public async Task ClearAllDataAsync()
        {
            try
            {
                await _realm.WriteAsync(() =>
                {
                    _realm.RemoveAll<Dispositivo>();
                });
                Debug.WriteLine("Todos os dados locais foram removidos com sucesso");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao limpar dados locais: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public void Dispose()
        {
            _realm?.Dispose();
        }
    }
}

