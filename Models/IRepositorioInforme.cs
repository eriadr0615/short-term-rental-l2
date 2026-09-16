
namespace Inmobiliaria.Models
{
    public interface IRepositorioInforme
    {
        IList<InformeInmueble> InmueblesConPropietario(bool? disponible);
         IList<InformeInmueble> InmueblesPorPropietario(string dni);
        IList<InformeInmueble> InmueblesMasReservados();
    }
}