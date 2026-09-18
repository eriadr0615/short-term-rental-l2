using System.ComponentModel.DataAnnotations;

namespace Inmobiliaria.Models
{

    public class ConfirmarReservaView
    {
        public Reserva Reserva { get; set; } = new Reserva();

        [Required(ErrorMessage = "El concepto del pago es obligatorio")]
        [StringLength(150)]
        public string Concepto { get; set; } = "Seña inicial";

        [Range(0, 9999999999.99, ErrorMessage = "El importe no puede ser negativo ni superar el límite permitido")]
        public decimal MontoPago { get; set; }
        public decimal MontoTotal { get; set; }

        public decimal PorcentajeReserva { get; set; }
        public decimal MontoMinimo { get; set; }
        public decimal SaldoRestante { get; set; }


    }

    public class FinalizarReservaView
    {
        public int IdReserva { get; set; }

        [Required(ErrorMessage = "La fecha de finalización es obligatoria")]
        [DataType(DataType.Date)]
        public DateTime FechaFinalizacion { get; set; }

        public decimal MultaCalculada { get; set; }
        public int PorcentajeMulta { get; set; }
        public int DiasRestantes { get; set; }
        public bool Calculada { get; set; }
        public DateTime? FechaCalculada { get; set; }
    }

    public class RenovarReservaView
    {
        public int IdReservaOrigen { get; set; }

        [Required(ErrorMessage = "La fecha de inicio es obligatoria")]
        [DataType(DataType.Date)]
        public DateTime FechaInicio { get; set; }

        [Required(ErrorMessage = "La fecha de finalización es obligatoria")]
        [DataType(DataType.Date)]
        public DateTime FechaFin { get; set; }

        [Range(0.01, 9999999999.99, ErrorMessage = "El monto diario debe ser mayor que cero")]
        public decimal MontoDia { get; set; }

    }
}
