namespace Inmobiliaria.Models
{
    public interface IRepositorioTipoInmueble : IRepositorio<TipoInmueble>
    {
        IList<TipoInmueble> ObtenerLista(
            int pagina,
            int tamanoPagina,
            string? buscar,
            out int totalRegistros);
    }
}
