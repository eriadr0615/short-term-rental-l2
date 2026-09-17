using Inmobiliaria.Models;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;

namespace Inmobiliaria.Controllers
{
    public class InformeController : Controller
    {
        private readonly IRepositorioInforme repositorio;
        public InformeController(IRepositorioInforme repositorio)
        {
            this.repositorio = repositorio;
        }


        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Inmueble(bool? disponible, int pagina = 1)
        {
            try
            {
                const int tamanoPagina = 10;
                pagina = Math.Max(pagina, 1);
                var lista = repositorio.InmueblesConPropietario(
                disponible, pagina, tamanoPagina, out int totalRegistros);
                ViewBag.Disponible = disponible;
                ViewBag.Pagina = pagina;
                ViewBag.TotalPaginas = (int)Math.Ceiling(totalRegistros / (double)tamanoPagina);
                return View(lista);
            }
            catch (MySqlException ex)
            {
                ViewBag.Error =
                    "Hubo un problema al consultar la base de datos";
                Console.WriteLine(ex.Message);
                return View(new List<InformeInmueble>());
            }
            catch (Exception ex)
            {
                ViewBag.Error =
                "Ups, hubo un error al generar el informe";
                Console.WriteLine(ex.Message);
                return View(new List<InformeInmueble>());
            }
        }


        public IActionResult SinReserva(int? dias)
        {
            try
            {
                IList<InformeInmueble> lista =
                    new List<InformeInmueble>();
                if (dias.HasValue && dias.Value > 0)
                {
                    lista = repositorio.InmueblesSinReserva(dias.Value);
                    ViewBag.BusquedaRealizada = true;
                }

                ViewBag.Dias = dias;
                return View(lista);
            }
            catch (MySqlException ex)
            {
                ViewBag.Error =
                    "Ups, hubo un problema al consultar la base de datos.";
                Console.WriteLine(ex.Message);
                return View(new List<InformeInmueble>());
            }
            catch (Exception ex)
            {
                ViewBag.Error =
                    "Ups, hubo un error al generar el informe.";
                Console.WriteLine(ex.Message);
                return View(new List<InformeInmueble>());
            }
        }




        public IActionResult Propietario(string? dni)
        {
            try
            {
                IList<InformeInmueble> lista =
                    new List<InformeInmueble>();
                if (!string.IsNullOrWhiteSpace(dni))
                {
                    lista = repositorio.InmueblesPorPropietario(dni);
                    ViewBag.BusquedaRealizada = true;
                }
                ViewBag.Dni = dni;
                return View(lista);
            }
            catch (MySqlException ex)
            {
                ViewBag.Error =
                    "Ups, hubo un problema al consultar la base de datos";
                Console.WriteLine(ex.Message);
                return View(new List<InformeInmueble>());
            }
            catch (Exception ex)
            {
                ViewBag.Error =
                    "Ups, hubo un error al generar el informe";
                Console.WriteLine(ex.Message);
                return View(new List<InformeInmueble>());
            }
        }


        public IActionResult MasReservado(int pagina = 1)
        {
            try
            {
                const int tamanoPagina = 10;
                pagina = Math.Max(pagina, 1);
                var lista = repositorio.InmueblesMasReservados(
                    pagina, tamanoPagina, out int totalRegistros);
                ViewBag.Pagina = pagina;
                ViewBag.TotalPaginas = (int)Math.Ceiling(totalRegistros / (double)tamanoPagina);

                return View(lista);
            }
            catch (MySqlException ex)
            {
                ViewBag.Error =
                    "Hubo un problema al consultar la base de datos";
                Console.WriteLine(ex.Message);
                return View(new List<InformeInmueble>());
            }
            catch (Exception ex)
            {
                ViewBag.Error =
                    "Hubo un error al generar el informe";
                Console.WriteLine(ex.Message);
                return View(new List<InformeInmueble>());
            }
        }
    }
}
