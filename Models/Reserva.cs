using System.ComponentModel.DataAnnotations;

namespace Inmobiliaria.Models
{
    public class Reserva
    {
        public int IdReserva { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un inquilino")]
        public int IdInquilino { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un inmueble")]
        public int IdInmueble { get; set; }
        [DataType(DataType.Date)]
        public DateTime FechaInicio { get; set; }
        [DataType(DataType.Date)]
        public DateTime FechaFinOriginal { get; set; }
        [Range(0.01, 9999999999.99, ErrorMessage = "El monto diario debe ser mayor que cero")]
        public decimal MontoDia { get; set; }
        public DateTime? FechaFinalizacionAnticipada { get; set; }
        public int? IdUsuarioCreacion { get; set; }
        public int? IdUsuarioFinalizacion { get; set; }
        public int? IdReservaOrigen { get; set; }
    }
}
