namespace Inmobiliaria.Models
{
    public interface IRepositorioPago : IRepositorio<Pago>
    {
        IList<Pago> ObtenerPorReserva(int idReserva);
        int Anular(int idPago, int idUsuarioAnulacion);
        IList<Pago> ObtenerLista(
            int pagina,
            int tamanoPagina,
            string? buscar,
            int? idReserva,
            out int totalRegistros);
    }
}
