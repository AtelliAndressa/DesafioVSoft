using System.ComponentModel.DataAnnotations;

namespace DeviceManager.Web.Models
{
    public class Dispositivo
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "Descrição obrigatória!")]
        [StringLength(100, ErrorMessage = "Máximo 100 caracteres")]
        public string Descricao { get; set; }

        [Required(ErrorMessage = "Código obrigatório!")]
        [RegularExpression(@"^[A-Z0-9]{5,10}$", ErrorMessage = "Formato inválido (5-10 letras/números)")]
        public string CodigoReferencia { get; set; }

        public DateTime DataCriacao { get; set; } = DateTime.Now;

        public DateTime? DataAtualizacao { get; set; }
    }
}
