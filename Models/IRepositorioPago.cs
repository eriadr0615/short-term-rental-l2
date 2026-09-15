namespace Inmobiliaria.Models
{
    public interface IRepositorioPago : IRepositorio<Pago>
    {
        IList<Pago> ObtenerPorReserva(int idReserva);
        int Anular(int idPago, int idUsuarioAnulacion);
    }
}
