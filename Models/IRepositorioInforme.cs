
namespace Inmobiliaria.Models
{
    public interface IRepositorioInforme
    {
        IList<InformeInmueble> InmueblesConPropietario(
            bool? disponible,
            int pagina,
            int tamanoPagina,
            out int totalRegistros);
         IList<InformeInmueble> InmueblesPorPropietario(string dni);
        IList<InformeInmueble> InmueblesMasReservados(
            int pagina,
            int tamanoPagina,
            out int totalRegistros);
    }
}
