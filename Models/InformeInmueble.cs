namespace Inmobiliaria.Models
{
    public class InformeInmueble
    {
        public int IdInmueble {get; set; }
        public string Direccion {get; set; } = "";
        public string Propietario {get; set; } = "";
        public string TipoInmueble {get; set; } = "";
        public decimal PrecioDiario {get; set; }
        public int CapacidadMaxima {get; set; }
        public bool Disponible {get; set; }
        public int CantidadReservas { get; set; }
    }
}