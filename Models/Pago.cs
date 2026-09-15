using System.ComponentModel.DataAnnotations;

namespace Inmobiliaria.Models
{
    public class Pago
    {
        public const string EstadoActivo = "ACTIVO";
        public const string EstadoAnulado = "ANULADO";

        public int IdPago { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una reserva")]
        public int IdReserva { get; set; }

        [Required(ErrorMessage = "El concepto es obligatorio")]
        [StringLength(150)]
        public string Concepto { get; set; } = "";

        [DataType(DataType.DateTime)]
        public DateTime FechaPago { get; set; }

        [Range(0.01, 9999999999.99,
            ErrorMessage = "El monto debe ser mayor que cero")]
        public decimal Monto { get; set; }

        public string Estado { get; set; } = EstadoActivo;

        public int IdUsuarioCreacion { get; set; }

        public int? IdUsuarioAnulacion { get; set; }
    }
}
