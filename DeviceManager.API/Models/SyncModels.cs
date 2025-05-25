using System.ComponentModel.DataAnnotations;

namespace DeviceManager.API.Models
{
    public class SyncModels
    {
        [Required]
        public List<SyncItemRequest> Items { get; set; }
    }

    public class SyncItemRequest
    {
        [Required]
        public string Operation { get; set; }

        [Required]
        public Dispositivo Device { get; set; }
    }

    public class SyncResult
    {
        public bool Success { get; set; }
        public List<SyncItem> SyncedItems { get; set; }
    }

    public class SyncItem
    {
        public string DeviceId { get; set; }
        public bool Success { get; set; }
        public string Error { get; set; }
    }
} 