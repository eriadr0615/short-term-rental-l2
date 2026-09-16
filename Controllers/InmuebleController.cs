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

        public IActionResult Index(int pagina = 1, string? buscar = null)
        {
            const int tamanoPagina = 10;
            pagina = Math.Max(pagina, 1);
            var lista = repositorio.ObtenerLista(
                pagina, tamanoPagina, buscar, out int totalRegistros);

            ViewBag.TiposInmueble = repositorioTipoInmueble.ObtenerLista();
            ViewBag.Pagina = pagina;
            ViewBag.TotalPaginas = (int)Math.Ceiling(totalRegistros / (double)tamanoPagina);
            ViewBag.Buscar = buscar;

            return View(lista);
        }

        [HttpGet]
        public IActionResult Buscar(
            string q,
            DateTime? fechaInicio,
            DateTime? fechaFin,
            int? idReservaExcluir)
        {
            if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 2)
            {
                return Json(Array.Empty<object>());
            }

            var resultado = repositorio.BuscarDisponibles(
                    q, fechaInicio, fechaFin, idReservaExcluir, 10)
                .Select(i => new
                {
                    id = i.IdInmueble,
                    texto = i.DireccionInmueble
                });
            return Json(resultado);
        }

        public IActionResult Create()
        {
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

            ViewBag.PropietarioActual = repositorioPropietario.ObtenerPorId(inmueble.IdPropietario);
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

            ViewBag.PropietarioActual = repositorioPropietario.ObtenerPorId(inmueble.IdPropietario);
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

            ViewBag.PropietarioActual = repositorioPropietario.ObtenerPorId(inmueble.IdPropietario);
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
