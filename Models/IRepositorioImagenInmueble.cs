namespace Inmobiliaria.Models
{
    public interface IRepositorioImagenInmueble : IRepositorio<ImagenInmueble>
    {
        IList<ImagenInmueble> ObtenerPorInmueble(int idInmueble);
        int QuitarPrincipal(int idInmueble);
    }
}