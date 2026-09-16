using Inmobiliaria.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inmobiliaria.Controllers
{
    [Authorize]
    public class TiposInmuebleController : Controller
    {
        private readonly IRepositorioTipoInmueble repositorio;
        public TiposInmuebleController(IRepositorioTipoInmueble repositorio)
        {
            this.repositorio = repositorio;
        }


        public IActionResult Index(int pagina = 1, string? buscar = null)
        {
            const int tamanoPagina = 10;
            pagina = Math.Max(pagina, 1);
            var lista = repositorio.ObtenerLista(
                pagina, tamanoPagina, buscar, out int totalRegistros);
            ViewBag.Pagina = pagina;
            ViewBag.TotalPaginas = (int)Math.Ceiling(totalRegistros / (double)tamanoPagina);
            ViewBag.Buscar = buscar;
            return View(lista);
        }

        // ALTA - mostrar formulario
        public IActionResult Create()
        {
            return View();
        }

        // ALTA - guardar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(TipoInmueble tipo)
        {
            if (ModelState.IsValid)
            {
                repositorio.Alta(tipo);
                return RedirectToAction("Index");
            }
            return View(tipo);
        }

        public IActionResult Edit(int id)
        {
            var tipo = repositorio.ObtenerPorId(id);

            if (tipo == null)
                return NotFound();

            return View(tipo);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(TipoInmueble tipo)
        {
            if (ModelState.IsValid)
            {
                repositorio.Modificacion(tipo);
                return RedirectToAction("Index");
            }
            return View(tipo);
        }

        [Authorize(Policy = Usuario.RolAdministrador)]
        public IActionResult Eliminar(int id)
        {
            var tipo = repositorio.ObtenerPorId(id);
            if (tipo == null)
                return NotFound();
            return View(tipo);
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
            var tipo = repositorio.ObtenerPorId(id);
            if (tipo == null)
                return NotFound();
            return View(tipo);
        }

    }
}
