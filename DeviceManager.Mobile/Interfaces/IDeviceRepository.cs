using DeviceManager.Mobile.Models;

namespace DeviceManager.Mobile.Interfaces
{
    public interface IDeviceRepository : IDisposable
    {
        Task<List<Dispositivo>> GetAllAsync();
        Task<Dispositivo> GetByIdAsync(string id);
        Task<Dispositivo> GetByCodigoReferenciaAsync(string codigoReferencia);
        Task AddAsync(Dispositivo dispositivo);
        Task UpdateAsync(Dispositivo dispositivo);
        Task DeleteAsync(Dispositivo dispositivo);
        Task<List<Dispositivo>> GetUnsynchronizedAsync();
        Task<bool> CodigoReferenciaExisteAsync(string codigoReferencia);
        Task<int> GetPendingChangesCountAsync();
        Task ClearAllDataAsync();
    }
}
