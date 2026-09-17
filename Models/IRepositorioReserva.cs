namespace Inmobiliaria.Models
{
    public interface IRepositorioReserva : IRepositorio<Reserva>
    {
        bool ExisteSuperposicion(
            int idInmueble,
            DateTime fechaInicio,
            DateTime fechaFin,
            int? idReservaExcluir);

        int AltaConPago(Reserva reserva, Pago? pagoInicial);

        int FinalizarConPago(Reserva reserva, DateTime fechaFinalizacion, Pago pagoMulta);

        IList<Reserva> ObtenerLista(
            int pagina,
            int tamanoPagina,
            string? buscar,
            out int totalRegistros);

        IList<Reserva> Buscar(string termino, int cantidad);
    }
}
