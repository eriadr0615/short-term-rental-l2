using Inmobiliaria.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;

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

        [HttpGet]
        public IActionResult Buscar(string q)
        {
            if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 2)
            {
                return Json(Array.Empty<object>());
            }

            var resultado = repositorio.Buscar(q, 10)
                .Select(p => new
                {
                    id = p.IdPropietario,
                    texto = $"{p.Nombre} {p.Apellido} - DNI {p.Dni}"
                });
            return Json(resultado);
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
                try
                {
                    propietario.Dni = propietario.Dni.Trim();
                    repositorio.Alta(propietario);
                    return RedirectToAction("Index");
                }
                catch (MySqlException ex) when (ex.Number == 1062)
                {
                    ModelState.AddModelError(nameof(propietario.Dni), "Ya existe un propietario con ese DNI");
                }
                catch (MySqlException)
                {
                    ModelState.AddModelError("", "No se pudo guardar el propietario. Intentá nuevamente.");
                }
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
            if (repositorio.ObtenerPorId(propietario.IdPropietario) == null)
                return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    propietario.Dni = propietario.Dni.Trim();
                    repositorio.Modificacion(propietario);
                    return RedirectToAction("Index");
                }
                catch (MySqlException ex) when (ex.Number == 1062)
                {
                    ModelState.AddModelError(nameof(propietario.Dni), "Ya existe un propietario con ese DNI");
                }
                catch (MySqlException)
                {
                    ModelState.AddModelError("", "No se pudo modificar el propietario. Intentá nuevamente.");
                }
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
            var propietario = repositorio.ObtenerPorId(id);
            if (propietario == null) return NotFound();
            try
            {
                repositorio.Baja(id);
                return RedirectToAction("Index");
            }
            catch (MySqlException ex) when (ex.Number == 1451)
            {
                ModelState.AddModelError("", "No se puede eliminar este propietario porque tiene inmuebles asociados");
            }
            catch (MySqlException)
            {
                ModelState.AddModelError("", "No se pudo eliminar el propietario. Intentá nuevamente.");
            }
            return View("Eliminar", propietario);
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
