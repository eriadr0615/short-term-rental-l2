namespace Inmobiliaria.Models
{
    // Datos combinados para los listados; no representa una tabla nueva.
    public class InformeReserva
    {
        public int IdReserva { get; set; }
        public string Inquilino { get; set; } = "";
        public string Direccion { get; set; } = "";
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFinOriginal { get; set; }
        public DateTime? FechaFinalizacionAnticipada { get; set; }
        public decimal MontoDia { get; set; }
        public DateTime FechaFinEfectiva => FechaFinalizacionAnticipada ?? FechaFinOriginal;

        // Nombres utilizados por las vistas de informes realizadas por el equipo.
        public string Inmueble => Direccion;
        public DateTime FechaFin => FechaFinEfectiva;
    }
}
