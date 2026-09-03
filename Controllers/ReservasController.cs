using Inmobiliaria.Models;
using Microsoft.AspNetCore.Mvc;

namespace Inmobiliaria.Controllers
{
    public class ReservasController : Controller
    {
        private readonly IRepositorioReserva repositorio;
        private readonly IRepositorioInquilino repositorioInquilino;
        private readonly IRepositorioInmueble repositorioInmueble;

        public ReservasController(
            IRepositorioReserva repositorio,
            IRepositorioInquilino repositorioInquilino,
            IRepositorioInmueble repositorioInmueble)
        {
            this.repositorio = repositorio;
            this.repositorioInquilino = repositorioInquilino;
            this.repositorioInmueble = repositorioInmueble;
        }

        public IActionResult Index()
        {
            var lista = repositorio.ObtenerLista();
            return View("~/Views/Reserva/Index.cshtml", lista);
        }

        public IActionResult Details(int id)
        {
            var reserva = repositorio.ObtenerPorId(id);

            if (reserva == null)
            {
                return NotFound();
            }

            return View("~/Views/Reserva/Details.cshtml", reserva);
        }

        public IActionResult Create()
        {
            ViewBag.Inquilinos = repositorioInquilino.ObtenerLista();
            ViewBag.Inmuebles = repositorioInmueble.ObtenerLista();

            return View("~/Views/Reserva/Create.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Reserva reserva)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Inquilinos = repositorioInquilino.ObtenerLista();
                ViewBag.Inmuebles = repositorioInmueble.ObtenerLista();

                return View("~/Views/Reserva/Create.cshtml", reserva);
            }

            repositorio.Alta(reserva);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            var reserva = repositorio.ObtenerPorId(id);

            if (reserva == null)
            {
                return NotFound();
            }

            ViewBag.Inquilinos = repositorioInquilino.ObtenerLista();
            ViewBag.Inmuebles = repositorioInmueble.ObtenerLista();

            return View("~/Views/Reserva/Edit.cshtml", reserva);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Reserva reserva)
        {
            if (id != reserva.IdReserva)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Inquilinos = repositorioInquilino.ObtenerLista();
                ViewBag.Inmuebles = repositorioInmueble.ObtenerLista();

                return View("~/Views/Reserva/Edit.cshtml", reserva);
            }

            repositorio.Modificacion(reserva);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Delete(int id)
        {
            var reserva = repositorio.ObtenerPorId(id);

            if (reserva == null)
            {
                return NotFound();
            }

            return View("~/Views/Reserva/Delete.cshtml", reserva);
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
