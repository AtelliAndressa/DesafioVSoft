using DeviceManager.Mobile.Interfaces;
using MvvmHelpers;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace DeviceManager.Mobile.ViewModels
{
    public class SyncViewModel : BaseViewModel
    {
        private readonly ISyncService _syncService;
        private string _syncStatus;
        private bool _isSyncing;
        private ObservableCollection<string> _syncLogs;
        private int _syncProgress;
        private int _totalItems;
        private int _processedItems;

        public string SyncStatus
        {
            get => _syncStatus;
            set => SetProperty(ref _syncStatus, value);
        }

        public bool IsSyncing
        {
            get => _isSyncing;
            set => SetProperty(ref _isSyncing, value);
        }

        public ObservableCollection<string> SyncLogs
        {
            get => _syncLogs;
            set => SetProperty(ref _syncLogs, value);
        }

        public int SyncProgress
        {
            get => _syncProgress;
            set => SetProperty(ref _syncProgress, value);
        }

        public ICommand SyncCommand { get; }

        public SyncViewModel(ISyncService syncService)
        {
            _syncService = syncService;
            SyncCommand = new Command(async () => await SyncAsync());
            SyncLogs = new ObservableCollection<string>();
        }

        private async Task SyncAsync()
        {
            if (IsSyncing) return;

            try
            {
                IsSyncing = true;
                SyncLogs.Clear();
                SyncStatus = "Iniciando sincronização...";
                SyncProgress = 0;
                _processedItems = 0;

                var progress = new Progress<SyncProgressInfo>(info =>
                {
                    UpdateSyncProgress(info);
                });

                var result = await _syncService.SyncAsync(progress);
                
                if (result)
                {
                    SyncStatus = "Sincronização concluída com sucesso!";
                    SyncLogs.Add("Sincronização concluída com sucesso!");
                }
                else
                {
                    SyncStatus = "Erro durante a sincronização.";
                    SyncLogs.Add("Erro durante a sincronização.");
                }
            }
            catch (Exception ex)
            {
                SyncStatus = $"Erro: {ex.Message}";
                SyncLogs.Add($"Erro: {ex.Message}");
            }
            finally
            {
                IsSyncing = false;
            }
        }

        private void UpdateSyncProgress(SyncProgressInfo info)
        {
            SyncLogs.Add(info.Message);
            SyncStatus = info.Message;

            if (info.TotalItems > 0)
            {
                _totalItems = info.TotalItems;
                _processedItems = info.ProcessedItems;
                SyncProgress = (int)((_processedItems * 100.0) / _totalItems);
            }
        }
    }

    public class SyncProgressInfo
    {
        public string Message { get; set; }
        public int TotalItems { get; set; }
        public int ProcessedItems { get; set; }
        public bool HasConflict { get; set; }
        public string ConflictDetails { get; set; }
    }
} 