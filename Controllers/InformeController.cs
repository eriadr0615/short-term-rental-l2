using Inmobiliaria.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;

namespace Inmobiliaria.Controllers
{
    [Authorize]
    public class InformeController : Controller
    {
        private const int TamanoPagina = 10;
        private readonly IRepositorioInforme repositorio;
        private readonly IRepositorioReserva repositorioReserva;

        public InformeController(IRepositorioInforme repositorio, IRepositorioReserva repositorioReserva)
        {
            this.repositorio = repositorio;
            this.repositorioReserva = repositorioReserva;
        }

        public IActionResult Index() => View();

        public IActionResult Inmueble(bool? disponible, int pagina = 1)
        {
            pagina = PrepararPaginacion(pagina);
            ViewBag.Disponible = disponible;
            IList<InformeInmueble> lista = new List<InformeInmueble>();
            try
            {
                if (ModelState.IsValid)
                {
                    lista = repositorio.InmueblesConPropietario(disponible, pagina, TamanoPagina, out int total);
                    MostrarTotalPaginas(total);
                }
            }
            catch (MySqlException ex) { ErrorConsulta(ex); }
            return View(lista);
        }

        public IActionResult Propietario(string? dni, int pagina = 1)
        {
            pagina = PrepararPaginacion(pagina);
            dni = dni?.Trim();
            ViewBag.Dni = dni;
            ViewBag.Consultado = !string.IsNullOrEmpty(dni);
            ViewBag.BusquedaRealizada = ViewBag.Consultado;
            IList<InformeInmueble> lista = new List<InformeInmueble>();
            if (dni?.Length > 15)
                ModelState.AddModelError("dni", "El DNI admite hasta 15 caracteres");
            try
            {
                if (!string.IsNullOrEmpty(dni) && ModelState.IsValid)
                {
                    lista = repositorio.InmueblesPorPropietario(dni, pagina, TamanoPagina, out int total);
                    MostrarTotalPaginas(total);
                }
            }
            catch (MySqlException ex) { ErrorConsulta(ex); }
            return View(lista);
        }

        public IActionResult MasReservado(int pagina = 1)
        {
            pagina = PrepararPaginacion(pagina);
            IList<InformeInmueble> lista = new List<InformeInmueble>();
            try
            {
                if (ModelState.IsValid)
                {
                    lista = repositorio.InmueblesMasReservados(pagina, TamanoPagina, out int total);
                    MostrarTotalPaginas(total);
                }
            }
            catch (MySqlException ex) { ErrorConsulta(ex); }
            return View(lista);
        }

        public IActionResult SinReserva(int dias = 30, int pagina = 1)
        {
            pagina = PrepararPaginacion(pagina);
            ViewBag.Dias = dias;
            ViewBag.BusquedaRealizada = true;
            IList<InformeInmueble> lista = new List<InformeInmueble>();
            try
            {
                if (ValidarPlazo(dias, futuro: false))
                {
                    ViewBag.Desde = DateTime.Today.AddDays(-dias);
                    lista = repositorio.InmueblesSinReserva(dias, pagina, TamanoPagina, out int total);
                    MostrarTotalPaginas(total);
                }
            }
            catch (MySqlException ex) { ErrorConsulta(ex); }
            return View(lista);
        }

        public IActionResult Reserva(int pagina = 1)
        {
            pagina = PrepararPaginacion(pagina);
            IList<InformeReserva> lista = new List<InformeReserva>();
            try
            {
                if (ModelState.IsValid)
                {
                    lista = repositorio.ReservasVigentes(pagina, TamanoPagina, out int total);
                    MostrarTotalPaginas(total);
                }
            }
            catch (MySqlException ex) { ErrorConsulta(ex); }
            return View(lista);
        }

        public IActionResult FinReserva(int dias = 30, int pagina = 1)
        {
            pagina = PrepararPaginacion(pagina);
            ViewBag.Dias = dias;
            ViewBag.BusquedaRealizada = true;
            IList<InformeReserva> lista = new List<InformeReserva>();
            try
            {
                if (ValidarPlazo(dias, futuro: true))
                {
                    ViewBag.Hasta = DateTime.Today.AddDays(dias);
                    lista = repositorio.ReservasPorFinalizar(dias, pagina, TamanoPagina, out int total);
                    MostrarTotalPaginas(total);
                }
            }
            catch (MySqlException ex) { ErrorConsulta(ex); }
            return View(lista);
        }

        public IActionResult Libres(DateTime? fechaInicio, DateTime? fechaFin, int pagina = 1)
        {
            pagina = PrepararPaginacion(pagina);
            ViewBag.FechaInicio = fechaInicio?.ToString("yyyy-MM-dd");
            ViewBag.FechaFin = fechaFin?.ToString("yyyy-MM-dd");
            ViewBag.Consultado = fechaInicio.HasValue || fechaFin.HasValue || !ModelState.IsValid;
            IList<InformeInmueble> lista = new List<InformeInmueble>();
            if (ViewBag.Consultado)
            {
                if (!fechaInicio.HasValue || !fechaFin.HasValue)
                    ModelState.AddModelError("", "Ingresá la fecha de inicio y la de finalización");
                else if (fechaInicio.Value.Year < 1000 || fechaFin.Value.Date <= fechaInicio.Value.Date)
                    ModelState.AddModelError("", "Las fechas deben ser válidas y la finalización posterior al inicio");
                try
                {
                    if (ModelState.IsValid)
                    {
                        lista = repositorio.InmueblesLibres(fechaInicio!.Value, fechaFin!.Value,
                            pagina, TamanoPagina, out int total);
                        MostrarTotalPaginas(total);
                    }
                }
                catch (MySqlException ex) { ErrorConsulta(ex); }
            }
            return View(lista);
        }

        public IActionResult Pagos(int? idReserva)
        {
            ViewBag.IdReserva = idReserva;
            if (idReserva.HasValue && idReserva.Value <= 0)
                ModelState.AddModelError("idReserva", "Ingresá un número de reserva válido");
            try
            {
                if (idReserva.HasValue && ModelState.IsValid)
                {
                    if (repositorioReserva.ObtenerPorId(idReserva.Value) == null)
                        ModelState.AddModelError("idReserva", "La reserva indicada no existe");
                    else
                        // El listado existente ya filtra, pagina y permite cargar pagos a esa reserva.
                        return RedirectToAction("Index", "Pagos", new { idReserva });
                }
            }
            catch (MySqlException ex) { ErrorConsulta(ex); }
            return View();
        }

        private int PrepararPaginacion(int pagina)
        {
            pagina = Math.Max(pagina, 1);
            ViewBag.Pagina = pagina;
            ViewBag.TotalPaginas = 0;
            return pagina;
        }

        private void MostrarTotalPaginas(int total)
        {
            ViewBag.TotalPaginas = (int)Math.Ceiling(total / (double)TamanoPagina);
        }

        private bool ValidarPlazo(int dias, bool futuro)
        {
            int maximo = futuro
                ? (DateTime.MaxValue.Date - DateTime.Today).Days
                : (DateTime.Today - new DateTime(1000, 1, 1)).Days;
            if (dias < 1 || dias > maximo)
                ModelState.AddModelError("dias", "Ingresá una cantidad de días positiva que forme un período de fechas válido");
            return ModelState.IsValid;
        }

        private void ErrorConsulta(MySqlException ex)
        {
            Console.WriteLine(ex.Message);
            ViewBag.Error = "No se pudo consultar el informe. Revisá la conexión a la base de datos e intentá nuevamente.";
            ModelState.AddModelError("", ViewBag.Error);
        }
    }
}
