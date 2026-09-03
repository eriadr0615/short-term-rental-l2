using Inmobiliaria.Models;
using Microsoft.AspNetCore.Mvc;

namespace Inmobiliaria.Controllers
{
    public class TiposInmuebleController : Controller
    {
        private readonly IRepositorioTipoInmueble repositorio;

        public TiposInmuebleController(IRepositorioTipoInmueble repositorio)
        {
            this.repositorio = repositorio;
        }

        // LISTAR
        public IActionResult Index()
        {
            var lista = repositorio.ObtenerLista();
            return View(lista);
        }

        // ALTA - mostrar formulario
        public IActionResult Create()
        {
            return View();
        }

        // ALTA - guardar
        [HttpPost]
        public IActionResult Create(TipoInmueble tipo)
        {
            if (ModelState.IsValid)
            {
                repositorio.Alta(tipo);
                return RedirectToAction("Index");
            }

            return View(tipo);
        }

        // MODIFICAR - mostrar formulario
        public IActionResult Edit(int id)
        {
            var tipo = repositorio.ObtenerPorId(id);

            if (tipo == null)
                return NotFound();

            return View(tipo);
        }

        // MODIFICAR - guardar
        [HttpPost]
        public IActionResult Edit(TipoInmueble tipo)
        {
            if (ModelState.IsValid)
            {
                repositorio.Modificacion(tipo);
                return RedirectToAction("Index");
            }

            return View(tipo);
        }

        // ELIMINAR - mostrar confirmación
        public IActionResult Eliminar(int id)
        {
            var tipo = repositorio.ObtenerPorId(id);

            if (tipo == null)
                return NotFound();

            return View(tipo);
        }

        // ELIMINAR - confirmar
        [HttpPost]
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