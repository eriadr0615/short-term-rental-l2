using System.ComponentModel.DataAnnotations;

namespace Inmobiliaria.Models
{
    public class Inmueble
    {
        public int IdInmueble { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un propietario")]
        public int IdPropietario { get; set; }

        [Required(ErrorMessage = "Ingrese una dirección valida")]
        [StringLength(150)]
        public string DireccionInmueble { get; set; } = "";

        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un tipo de inmueble")]
        public int IdTipoInmueble { get; set; }

        [StringLength(100)]
        public string CoordenadasInmuebles { get; set; } = "";
        //  importante! usar siempre decimal para precios y montos, no float!

        [Required(ErrorMessage = "Ingrese el precio por noche")]
        [Range(0.01, 9999999999.99, ErrorMessage = "El precio diario debe ser mayor que cero")]
        public decimal PrecioDiario { get; set; }

        [Required(ErrorMessage = "Ingrese un porcentaje de reserva")]
        [Range(0, 100, ErrorMessage = "El porcentaje de reserva debe estar entre 0 y 100")]
        public decimal PorcentajeReserva { get; set; }

        public bool Disponible { get; set; }

        [Required(ErrorMessage = "Ingrese la capacidad maxima del inmueble")]
        [Range(1, int.MaxValue, ErrorMessage = "La capacidad debe ser mayor que cero")]
        public int CapacidadMaxima { get; set; }
    }
}
