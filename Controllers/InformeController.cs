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
        public IActionResult Inmueble(bool? disponible)
        {
            try
            {
                var lista = repositorio.InmueblesConPropietario(disponible);
                ViewBag.Disponible = disponible;
                return View(lista);
            }
            catch (MySqlException ex)
            {
                ViewBag.Error =
                    "Hubo un problema al consultar la base de datos.";
                Console.WriteLine(ex.Message);
                return View(new List<InformeInmueble>());
            }
            catch (Exception ex)
            {
                ViewBag.Error =
                    "Hubo un error al generar el informe.";
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
                    "Hubo un problema al consultar la base de datos.";
                Console.WriteLine(ex.Message);
                return View(new List<InformeInmueble>());
            }
            catch (Exception ex)
            {
                ViewBag.Error =
                    "Hubo un error al generar el informe.";
                Console.WriteLine(ex.Message);
                return View(new List<InformeInmueble>());
            }
        }





        public IActionResult MasReservado()
        {
            try
            {
                var lista = repositorio.InmueblesMasReservados();

                return View(lista);
            }
            catch (MySqlException ex)
            {
                ViewBag.Error =
                    "Hubo un problema al consultar la base de datos.";

                Console.WriteLine(ex.Message);

                return View(new List<InformeInmueble>());
            }
            catch (Exception ex)
            {
                ViewBag.Error =
                    "Hubo un error al generar el informe.";
                Console.WriteLine(ex.Message);
                return View(new List<InformeInmueble>());
            }
        }
    }
}