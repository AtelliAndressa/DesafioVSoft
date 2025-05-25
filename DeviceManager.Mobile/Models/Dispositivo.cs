using Realms;

namespace DeviceManager.Mobile.Models
{
    public class Dispositivo : RealmObject
    {
        [PrimaryKey]
        public string ID { get; set; }

        public string Descricao { get; set; }

        [Indexed]
        public string CodigoReferencia { get; set; }

        public DateTimeOffset DataCriacao { get; set; } = DateTimeOffset.Now;

        public DateTimeOffset? DataAtualizacao { get; set; }

        public bool IsSynchronized { get; set; } = false;

        public bool IsDeleted { get; set; } = false;
    }

}
