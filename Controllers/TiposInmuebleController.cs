using Inmobiliaria.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;

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
                try
                {
                    tipo.NombreTipo = tipo.NombreTipo.Trim();
                    repositorio.Alta(tipo);
                    return RedirectToAction("Index");
                }
                catch (MySqlException ex) when (ex.Number == 1062)
                {
                    ModelState.AddModelError(nameof(tipo.NombreTipo), "Ya existe un tipo de inmueble con ese nombre");
                }
                catch (MySqlException)
                {
                    ModelState.AddModelError("", "No se pudo guardar el tipo de inmueble. Intentá nuevamente.");
                }
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
            if (repositorio.ObtenerPorId(tipo.IdTipoInmueble) == null)
                return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    tipo.NombreTipo = tipo.NombreTipo.Trim();
                    repositorio.Modificacion(tipo);
                    return RedirectToAction("Index");
                }
                catch (MySqlException ex) when (ex.Number == 1062)
                {
                    ModelState.AddModelError(nameof(tipo.NombreTipo), "Ya existe un tipo de inmueble con ese nombre");
                }
                catch (MySqlException)
                {
                    ModelState.AddModelError("", "No se pudo modificar el tipo de inmueble. Intentá nuevamente.");
                }
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
            var tipo = repositorio.ObtenerPorId(id);
            if (tipo == null) return NotFound();
            try
            {
                repositorio.Baja(id);
                return RedirectToAction("Index");
            }
            catch (MySqlException ex) when (ex.Number == 1451)
            {
                ModelState.AddModelError("", "No se puede eliminar este tipo porque tiene inmuebles asociados");
            }
            catch (MySqlException)
            {
                ModelState.AddModelError("", "No se pudo eliminar el tipo de inmueble. Intentá nuevamente.");
            }
            return View("Eliminar", tipo);
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
