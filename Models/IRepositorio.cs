namespace Inmobiliaria.Models
{
    public interface IRepositorio<T>
    {
        int Alta(T entidad);
        int Baja(int id);
        int Modificacion(T entidad);
        IList<T> ObtenerLista();
        T? ObtenerPorId(int id);
    }
}