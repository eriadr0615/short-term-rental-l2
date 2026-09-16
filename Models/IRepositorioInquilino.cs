namespace Inmobiliaria.Models
{
    public interface IRepositorioInquilino : IRepositorio<Inquilino>
    {
        IList<Inquilino> ObtenerLista(
            int pagina,
            int tamanoPagina,
            string? buscar,
            out int totalRegistros);

        IList<Inquilino> Buscar(string termino, int cantidad);
    }
}
