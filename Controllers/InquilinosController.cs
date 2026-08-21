using Inmobiliaria.Models;
using Microsoft.AspNetCore.Mvc;

namespace Inmobiliaria.Controllers
{
    public class InquilinosController : Controller
    {
        private readonly IRepositorioInquilino repositorio;

        public InquilinosController(IRepositorioInquilino repositorio)
        {
            this.repositorio = repositorio;
        }

        public IActionResult Index()
        {
            var lista = repositorio.ObtenerLista();
            return View("~/Views/Inquilino/Index.cshtml", lista);
        }

        public IActionResult Details(int id)
        {
            var inquilino = repositorio.ObtenerPorId(id);

            if (inquilino == null)
            {
                return NotFound();
            }

            return View("~/Views/Inquilino/Details.cshtml", inquilino);
        }

        public IActionResult Create()
        {
            return View("~/Views/Inquilino/Create.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Inquilino inquilino)
        {
            if (!ModelState.IsValid)
            {
                return View("~/Views/Inquilino/Create.cshtml", inquilino);
            }

            repositorio.Alta(inquilino);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            var inquilino = repositorio.ObtenerPorId(id);

            if (inquilino == null)
            {
                return NotFound();
            }

            return View("~/Views/Inquilino/Edit.cshtml", inquilino);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Inquilino inquilino)
        {
            if (id != inquilino.IdInquilino)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View("~/Views/Inquilino/Edit.cshtml", inquilino);
            }

            repositorio.Modificacion(inquilino);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Delete(int id)
        {
            var inquilino = repositorio.ObtenerPorId(id);

            if (inquilino == null)
            {
                return NotFound();
            }

            return View("~/Views/Inquilino/Delete.cshtml", inquilino);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            repositorio.Baja(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
