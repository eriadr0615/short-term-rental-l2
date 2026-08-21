using System.ComponentModel.DataAnnotations;

namespace Inmobiliaria.Models
{
    public class Propietario
    {
        public int IdPropietario { get; set; }

        [Required]
        public string Dni { get; set; } = "";

        [Required]
        public string Nombre { get; set; } = "";

        [Required]
        public string Apellido { get; set; } = "";

        public string Telefono { get; set; } = "";

        [EmailAddress]
        public string Correo { get; set; } = "";

        public string Direccion { get; set; } = "";
    }
}