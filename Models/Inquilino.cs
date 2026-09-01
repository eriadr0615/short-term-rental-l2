using System.ComponentModel.DataAnnotations;

namespace Inmobiliaria.Models
{
    public class Inquilino
    {
        public int IdInquilino { get; set; }
        // reviaar la validacion de DNI duplicado , en BD  es unique. Lo mismo en propietario
        [Required(ErrorMessage = "el DNI es obligatorio")]
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