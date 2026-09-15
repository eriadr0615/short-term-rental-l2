using Inmobiliaria.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inmobiliaria.Controllers
{
    [Authorize]
    public class InmuebleController : Controller
    {
        private readonly IRepositorioInmueble repositorio;
        private readonly IRepositorioPropietario repositorioPropietario;
        private readonly IRepositorioTipoInmueble repositorioTipoInmueble;
        private readonly IRepositorioImagenInmueble repositorioImagen;

        public InmuebleController(
            IRepositorioInmueble repositorio,
            IRepositorioPropietario repositorioPropietario,
            IRepositorioTipoInmueble repositorioTipoInmueble,
            IRepositorioImagenInmueble repositorioImagen)
        {
            this.repositorio = repositorio;
            this.repositorioPropietario = repositorioPropietario;
            this.repositorioTipoInmueble = repositorioTipoInmueble;
            this.repositorioImagen = repositorioImagen;
        }

        public IActionResult Index()
        {
            var lista = repositorio.ObtenerLista();

            ViewBag.TiposInmueble = repositorioTipoInmueble.ObtenerLista();

            return View(lista);
        }

        public IActionResult Create()
        {
            ViewBag.Propietarios = repositorioPropietario.ObtenerLista();
            ViewBag.TiposInmueble = repositorioTipoInmueble.ObtenerLista();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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
            {
                return NotFound();
            }

            ViewBag.Propietarios = repositorioPropietario.ObtenerLista();
            ViewBag.TiposInmueble = repositorioTipoInmueble.ObtenerLista();

            return View(inmueble);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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

        [Authorize(Policy = Usuario.RolAdministrador)]
        public IActionResult Eliminar(int id)
        {
            var inmueble = repositorio.ObtenerPorId(id);

            if (inmueble == null)
            {
                return NotFound();
            }

            return View(inmueble);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Usuario.RolAdministrador)]
        public IActionResult EliminarConfirmado(int id)
        {
            repositorio.Baja(id);

            return RedirectToAction("Index");
        }
        public IActionResult Details(int id)
        {
            var inmueble = repositorio.ObtenerPorId(id);

            if (inmueble == null)
            {
                return NotFound();
            }

            ViewBag.Imagenes = repositorioImagen.ObtenerPorInmueble(id);

            ViewBag.TipoInmueble =
                repositorioTipoInmueble.ObtenerPorId(inmueble.IdTipoInmueble);
            return View(inmueble);
        }


    }
}
