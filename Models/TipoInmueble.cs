using System.ComponentModel.DataAnnotations;

namespace Inmobiliaria.Models
{
    public class TipoInmueble
    {
        public int IdTipoInmueble { get; set; }

        [Required(ErrorMessage = "dato obligatorio. Ingrese el tipo de inmueble")]
        public string NombreTipo { get; set; } = "";
    }
} 