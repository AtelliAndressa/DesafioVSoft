using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DeviceManager.API.Models
{
    public class Dispositivo
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public string Id { get; set; }

        [BsonElement("descricao")]
        [Required]
        public string Descricao { get; set; }

        [BsonElement("codigoReferencia")]
        [Required]
        public string CodigoReferencia { get; set; }

        [BsonElement("dataCriacao")]
        public DateTime DataCriacao { get; set; }

        [BsonElement("dataAtualizacao")]
        public DateTime? DataAtualizacao { get; set; }

        [BsonElement("isDeleted")]
        public bool IsDeleted { get; set; }
    }
}
