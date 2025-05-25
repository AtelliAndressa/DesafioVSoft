namespace DeviceManager.API.Models
{
    public class SyncRequest
    {
        public List<Dispositivo> Novos { get; set; } = new();

        public List<Dispositivo> Atualizados { get; set; } = new();

        public List<string> Excluidos { get; set; } = new();
    }
}
