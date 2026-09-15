using System.ComponentModel.DataAnnotations;

namespace Inmobiliaria.Models
{
    public class Usuario
    {
        public const string RolAdministrador = "Administrador";
        public const string RolEmpleado = "Empleado";

        public int IdUsuario { get; set; }

        public string? Avatar { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100)]
        public string NombreUsuario { get; set; } = "";

        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "El correo no es válido")]
        [StringLength(100)]
        public string CorreoUsuario { get; set; } = "";

        public string ContraseniaHash { get; set; } = "";

        [Required]
        public string RolUsuario { get; set; } = RolEmpleado;

        public bool Activo { get; set; } = true;
    }
}
