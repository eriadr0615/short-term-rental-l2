namespace Inmobiliaria.Models
{
    public interface IRepositorioInmueble : IRepositorio<Inmueble>
    {
        IList<Inmueble> ObtenerLista(
            int pagina,
            int tamanoPagina,
            string? buscar,
            out int totalRegistros);

        IList<Inmueble> BuscarDisponibles(
            string termino,
            DateTime? fechaInicio,
            DateTime? fechaFin,
            int? idReservaExcluir,
            int cantidad);
    }
}
