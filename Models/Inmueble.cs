using System.ComponentModel.DataAnnotations;

namespace Inmobiliaria.Models
{
    public class Inmueble
    {
        public int IdInmueble { get; set; }

        public int IdPropietario { get; set; }

        [Required(ErrorMessage = "Ingrese una dirección valida")]
        public string DireccionInmueble { get; set; } = "";

        public int IdTipoInmueble { get; set; }

        public string CoordenadasInmuebles { get; set; } = "";
        //  importante! usar siempre decimal para precios y montos, no float!

        [Required(ErrorMessage = "Ingrese el precio por noche")]
        public decimal PrecioDiario { get; set; }

        [Required(ErrorMessage = "Ingrese un porcentaje de reserva")]
        public decimal PorcentajeReserva { get; set; }

        public bool Disponible { get; set; }

        [Required(ErrorMessage = "Ingrese la capacidad maxima del inmueble")]
        public int CapacidadMaxima { get; set; }
    }
}