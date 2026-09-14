namespace Inmobiliaria.Models
{
    public interface IRepositorioReserva : IRepositorio<Reserva>
    {
        bool ExisteSuperposicion(
            int idInmueble,
            DateTime fechaInicio,
            DateTime fechaFin,
            int? idReservaExcluir);
    }
}
