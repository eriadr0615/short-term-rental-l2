using System.ComponentModel.DataAnnotations;

namespace Inmobiliaria.Models
{
    public class Propietario
    {
        public int IdPropietario { get; set; }

        [Required(ErrorMessage = "el dni es obligatorio")]
        public string Dni { get; set; } = "";

        [Required(ErrorMessage = "el nombre es obligatorio")]
        public string Nombre { get; set; } = "";

        [Required(ErrorMessage = "el apellido es obligatorio")]
        public string Apellido { get; set; } = "";

        public string Telefono { get; set; } = "";

        [EmailAddress(ErrorMessage = "correo invalido")]
        public string Correo { get; set; } = "";

        public string Direccion { get; set; } = "";
    }
}