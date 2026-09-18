using Inmobiliaria.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;

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
            if (!ModelState.IsValid
                || string.IsNullOrWhiteSpace(q) || q.Trim().Length < 2
                || (fechaInicio.HasValue && fechaFin.HasValue && fechaFin <= fechaInicio))
            {
                return Json(Array.Empty<object>());
            }

            var resultado = repositorio.BuscarDisponibles(
        q, fechaInicio, fechaFin, idReservaExcluir, 10)
    .Select(i =>
    {
        var propietario =
            repositorioPropietario.ObtenerPorId(i.IdPropietario);

        var tipo =
            repositorioTipoInmueble.ObtenerPorId(i.IdTipoInmueble);
        //ampliacion de txt en cuadro de bsuqueda, sino es imposible recordar la dire del inmueble. Se amplia la bsuq a los atributos
        return new
        {
            id = i.IdInmueble,
            texto =
                        $"{i.DireccionInmueble} - " +
                        $"{tipo?.NombreTipo} - " +
                        $"{propietario?.Nombre} {propietario?.Apellido}",
                         precio = i.PrecioDiario
        };
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
            ValidarRelaciones(inmueble);
            if (ModelState.IsValid)
            {
                try
                {
                    repositorio.Alta(inmueble);
                    return RedirectToAction("Index");
                }
                catch (MySqlException)
                {
                    ModelState.AddModelError("", "No se pudo guardar el inmueble. Revisá los datos seleccionados e intentá nuevamente.");
                }
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
            if (repositorio.ObtenerPorId(inmueble.IdInmueble) == null)
                return NotFound();

            ValidarRelaciones(inmueble);
            if (ModelState.IsValid)
            {
                try
                {
                    repositorio.Modificacion(inmueble);
                    return RedirectToAction("Index");
                }
                catch (MySqlException)
                {
                    ModelState.AddModelError("", "No se pudo modificar el inmueble. Revisá los datos seleccionados e intentá nuevamente.");
                }
            }

            ViewBag.PropietarioActual = repositorioPropietario.ObtenerPorId(inmueble.IdPropietario);
            ViewBag.TiposInmueble = repositorioTipoInmueble.ObtenerLista();

            return View(inmueble);
        }

        private void ValidarRelaciones(Inmueble inmueble)
        {
            if (repositorioPropietario.ObtenerPorId(inmueble.IdPropietario) == null)
                ModelState.AddModelError(nameof(inmueble.IdPropietario), "El propietario seleccionado no existe");
            if (repositorioTipoInmueble.ObtenerPorId(inmueble.IdTipoInmueble) == null)
                ModelState.AddModelError(nameof(inmueble.IdTipoInmueble), "El tipo de inmueble seleccionado no existe");
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
            var inmueble = repositorio.ObtenerPorId(id);
            if (inmueble == null) return NotFound();
            try
            {
                repositorio.Baja(id);
                return RedirectToAction("Index");
            }
            catch (MySqlException ex) when (ex.Number == 1451)
            {
                ModelState.AddModelError("", "No se puede eliminar este inmueble porque tiene reservas asociadas. Si no debe ofrecerse, desmarcá Disponible.");
            }
            catch (MySqlException)
            {
                ModelState.AddModelError("", "No se pudo eliminar el inmueble. Intentá nuevamente.");
            }
            return View("Eliminar", inmueble);
        }
        public IActionResult Details(int id)
        {
            var inmueble = repositorio.ObtenerPorId(id);

            if (inmueble == null)
            {
                return NotFound();
            }

            ViewBag.Imagenes = repositorioImagen.ObtenerPorInmueble(id);
            ViewBag.Propietario = repositorioPropietario.ObtenerPorId(inmueble.IdPropietario);

            ViewBag.TipoInmueble =
                repositorioTipoInmueble.ObtenerPorId(inmueble.IdTipoInmueble);
            return View(inmueble);
        }


    }
}
