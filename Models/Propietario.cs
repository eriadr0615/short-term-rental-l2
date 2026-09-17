using System.ComponentModel.DataAnnotations;

namespace Inmobiliaria.Models
{
    public class Propietario
    {
        public int IdPropietario { get; set; }

        [Required(ErrorMessage = "el dni es obligatorio")]
        [StringLength(15, ErrorMessage = "El DNI no puede superar los 15 caracteres.")]
        public string Dni { get; set; } = "";

        [Required(ErrorMessage = "el nombre es obligatorio")]
        [StringLength(50, ErrorMessage = "El nombre no puede superar los 50 caracteres.")]
        public string Nombre { get; set; } = "";

        [Required(ErrorMessage = "el apellido es obligatorio")]
        [StringLength(50, ErrorMessage = "El apellido no puede superar los 50 caracteres.")]
        public string Apellido { get; set; } = "";

        [StringLength(30, ErrorMessage = "El teléfono no puede superar los 30 caracteres.")]
        public string Telefono { get; set; } = "";

        [EmailAddress(ErrorMessage = "correo invalido")]
        [StringLength(100, ErrorMessage = "El correo no puede superar los 100 caracteres.")]
        public string Correo { get; set; } = "";

        [StringLength(150, ErrorMessage = "La dirección no puede superar los 150 caracteres.")]
        public string Direccion { get; set; } = "";
    }
}
