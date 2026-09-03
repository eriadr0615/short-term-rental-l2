using Inmobiliaria.Models;
using Microsoft.AspNetCore.Mvc;

namespace Inmobiliaria.Controllers
{
    public class InmuebleController : Controller
    {
        private readonly IRepositorioInmueble repositorio;
        private readonly IRepositorioPropietario repositorioPropietario;
        private readonly IRepositorioTipoInmueble repositorioTipoInmueble;

        public InmuebleController(
            IRepositorioInmueble repositorio,
            IRepositorioPropietario repositorioPropietario,
            IRepositorioTipoInmueble repositorioTipoInmueble)
        {
            this.repositorio = repositorio;
            this.repositorioPropietario = repositorioPropietario;
            this.repositorioTipoInmueble = repositorioTipoInmueble;
        }

        public IActionResult Index()
        {
            var lista = repositorio.ObtenerLista();
            return View(lista);
        }

        public IActionResult Create()
        {
            ViewBag.Propietarios = repositorioPropietario.ObtenerLista();
            ViewBag.TiposInmueble = repositorioTipoInmueble.ObtenerLista();
            return View();
        }

        [HttpPost]
        public IActionResult Create(Inmueble inmueble)
        {
            if (ModelState.IsValid)
            {
                repositorio.Alta(inmueble);
                return RedirectToAction("Index");
            }

            ViewBag.Propietarios = repositorioPropietario.ObtenerLista();
            ViewBag.TiposInmueble = repositorioTipoInmueble.ObtenerLista();

            return View(inmueble);
        }


        public IActionResult Edit(int id)
        {
            var inmueble = repositorio.ObtenerPorId(id);

            if (inmueble == null)
                return NotFound();

            ViewBag.Propietarios = repositorioPropietario.ObtenerLista();
            ViewBag.TiposInmueble = repositorioTipoInmueble.ObtenerLista();

            return View(inmueble);
        }

        [HttpPost]
        public IActionResult Edit(Inmueble inmueble)
        {
            if (ModelState.IsValid)
            {
                repositorio.Modificacion(inmueble);
                return RedirectToAction("Index");
            }

            ViewBag.Propietarios = repositorioPropietario.ObtenerLista();
            ViewBag.TiposInmueble = repositorioTipoInmueble.ObtenerLista();

            return View(inmueble);
        }

        public IActionResult Eliminar(int id)
        {
            var inmueble = repositorio.ObtenerPorId(id);

            if (inmueble == null)
                return NotFound();

            return View(inmueble);
        }

        [HttpPost]
        public IActionResult EliminarConfirmado(int id)
        {
            repositorio.Baja(id);
            return RedirectToAction("Index");
        }
    }
}