namespace Inmobiliaria.Models
{
    public class InformeReserva
    {
        public int IdReserva { get; set; }
        public string Inquilino { get; set; } = "";
        public string Inmueble { get; set; } = "";
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public decimal MontoDia { get; set; }
    }
}