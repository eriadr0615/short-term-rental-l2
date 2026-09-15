namespace Inmobiliaria.Models
{
    public interface IRepositorioUsuario : IRepositorio<Usuario>
    {
        Usuario? ObtenerPorCorreo(string correo);
        int ActualizarClave(int id, string contraseniaHash);
        int ActualizarAvatar(int id, string? avatar);
        int CambiarEstado(int id, bool activo);
    }
}
