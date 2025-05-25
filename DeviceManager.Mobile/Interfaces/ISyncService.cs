using DeviceManager.Mobile.Models;
using DeviceManager.Mobile.ViewModels;

namespace DeviceManager.Mobile.Interfaces
{
    public interface ISyncService
    {
        Task<bool> SyncAsync(IProgress<SyncProgressInfo> progress = null);

        Task<List<Dispositivo>> GetPendingChangesAsync();

        Task<bool> HasPendingChangesAsync();

        Task<bool> ClearAndSyncAsync(IProgress<SyncProgressInfo> progress = null);
    }
} 