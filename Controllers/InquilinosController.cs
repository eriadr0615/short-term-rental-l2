using Inmobiliaria.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inmobiliaria.Controllers
{
    [Authorize]
    public class InquilinosController : Controller
    {
        private readonly IRepositorioInquilino repositorio;

        public InquilinosController(IRepositorioInquilino repositorio)
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
            return View("~/Views/Inquilino/Index.cshtml", lista);
        }

        [HttpGet]
        public IActionResult Buscar(string q)
        {
            if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 2)
            {
                return Json(Array.Empty<object>());
            }

            var resultado = repositorio.Buscar(q, 10)
                .Select(i => new
                {
                    id = i.IdInquilino,
                    texto = $"{i.Nombre} {i.Apellido} - DNI {i.Dni}"
                });
            return Json(resultado);
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

        [Authorize(Policy = Usuario.RolAdministrador)]
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
        [Authorize(Policy = Usuario.RolAdministrador)]
        public IActionResult DeleteConfirmed(int id)
        {
            repositorio.Baja(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
