using System.ComponentModel.DataAnnotations;

namespace Inmobiliaria.Models
{
    public class PagoEditarView
    {
        public int IdPago { get; set; }

        [Required(ErrorMessage = "El concepto es obligatorio")]
        [StringLength(150)]
        public string Concepto { get; set; } = "";
    }
}
