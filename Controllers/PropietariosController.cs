using Inmobiliaria.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inmobiliaria.Controllers
{
    [Authorize]
    public class PropietariosController : Controller
    {
        private readonly IRepositorioPropietario repositorio;

        public PropietariosController(IRepositorioPropietario repositorio)
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

        // ALlta - save
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Propietario propietario)
        {
            if (ModelState.IsValid)
            {
                repositorio.Alta(propietario);
                return RedirectToAction("Index");
            }

            return View(propietario);
        }

        // M- mostrar el form.
        public IActionResult Edit(int id)
        {
            var propietario = repositorio.ObtenerPorId(id);

            if (propietario == null)
                return NotFound();

            return View(propietario);
        }

        // Mod - save
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Propietario propietario)
        {
            if (ModelState.IsValid)
            {
                repositorio.Modificacion(propietario);
                return RedirectToAction("Index");
            }

            return View(propietario);
        }

        // deletes . muetsra la confirmacion  
        [Authorize(Policy = Usuario.RolAdministrador)]
        public IActionResult Eliminar(int id)
        {
            var propietario = repositorio.ObtenerPorId(id);

            if (propietario == null)
                return NotFound();

            return View(propietario);
        }

        // delete . confirmacion 
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
            var propietario = repositorio.ObtenerPorId(id);
            if (propietario == null)
                return NotFound();
            return View(propietario);
        }

    }
}
