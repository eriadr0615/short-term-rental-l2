namespace Inmobiliaria.Models
{
    public class Reserva
    {
        public int IdReserva { get; set; }
        public int IdInquilino { get; set; }
        public int IdInmueble { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFinOriginal { get; set; }
        public decimal MontoDia { get; set; }
        public DateTime? FechaFinalizacionAnticipada { get; set; }
        public int? IdUsuarioCreacion { get; set; }
        public int? IdUsuarioFinalizacion { get; set; }
        public int? IdReservaOrigen { get; set; }
    }
}
