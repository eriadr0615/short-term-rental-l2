namespace Inmobiliaria.Models
{
    //se elige idinmuble xq 1 inmuble: N imagnes
    public interface IRepositorioImagenInmueble : IRepositorio<ImagenInmueble>
    {
        IList<ImagenInmueble> ObtenerPorInmueble(int idInmueble);
    }
}