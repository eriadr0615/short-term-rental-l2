using System.ComponentModel.DataAnnotations;

namespace Inmobiliaria.Models
{
    public class TipoInmueble
    {
        public int IdTipoInmueble { get; set; }

        [Required(ErrorMessage = "dato obligatorio. Ingrese el tipo de inmueble")]
        [StringLength(50, ErrorMessage = "El tipo de inmueble no puede superar los 50 caracteres.")]
        public string NombreTipo { get; set; } = "";
    }
}
