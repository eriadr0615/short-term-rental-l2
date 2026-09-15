using System.ComponentModel.DataAnnotations;

namespace Inmobiliaria.Models
{
    public class LoginView
    {
        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "El correo no es válido")]
        public string Correo { get; set; } = "";

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [DataType(DataType.Password)]
        public string Clave { get; set; } = "";

        public bool Recordarme { get; set; }

        public string? ReturnUrl { get; set; }
    }

    public class UsuarioCrearView
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100)]
        public string NombreUsuario { get; set; } = "";

        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "El correo no es válido")]
        [StringLength(100)]
        public string CorreoUsuario { get; set; } = "";

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
        [DataType(DataType.Password)]
        public string Clave { get; set; } = "";

        [Required(ErrorMessage = "Debe repetir la contraseña")]
        [Compare(nameof(Clave), ErrorMessage = "Las contraseñas no coinciden")]
        [DataType(DataType.Password)]
        public string ConfirmarClave { get; set; } = "";

        [Required]
        public string RolUsuario { get; set; } = Usuario.RolEmpleado;
    }

    public class UsuarioEditarView
    {
        public int IdUsuario { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100)]
        public string NombreUsuario { get; set; } = "";

        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "El correo no es válido")]
        [StringLength(100)]
        public string CorreoUsuario { get; set; } = "";

        [Required]
        public string RolUsuario { get; set; } = Usuario.RolEmpleado;

        public bool Activo { get; set; }

        [MinLength(8, ErrorMessage = "La nueva contraseña debe tener al menos 8 caracteres")]
        [DataType(DataType.Password)]
        public string? NuevaClave { get; set; }
    }

    public class UsuarioPerfilView
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100)]
        public string NombreUsuario { get; set; } = "";

        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "El correo no es válido")]
        [StringLength(100)]
        public string CorreoUsuario { get; set; } = "";
    }

    public class CambiarClaveView
    {
        [Required(ErrorMessage = "La contraseña actual es obligatoria")]
        [DataType(DataType.Password)]
        public string ClaveActual { get; set; } = "";

        [Required(ErrorMessage = "La nueva contraseña es obligatoria")]
        [MinLength(8, ErrorMessage = "La nueva contraseña debe tener al menos 8 caracteres")]
        [DataType(DataType.Password)]
        public string NuevaClave { get; set; } = "";

        [Required(ErrorMessage = "Debe repetir la nueva contraseña")]
        [Compare(nameof(NuevaClave), ErrorMessage = "Las contraseñas no coinciden")]
        [DataType(DataType.Password)]
        public string ConfirmarClave { get; set; } = "";
    }

    public class AvatarView
    {
        public string? AvatarActual { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una imagen")]
        public IFormFile? Archivo { get; set; }
    }
}
