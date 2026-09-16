using System.Security.Claims;
using Inmobiliaria.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inmobiliaria.Controllers
{
    [Authorize]
    public class PagosController : Controller
    {
        private readonly IRepositorioPago repositorio;
        private readonly IRepositorioReserva repositorioReserva;
        private readonly IRepositorioInquilino repositorioInquilino;
        private readonly IRepositorioInmueble repositorioInmueble;
        private readonly IRepositorioUsuario repositorioUsuario;

        public PagosController(
            IRepositorioPago repositorio,
            IRepositorioReserva repositorioReserva,
            IRepositorioInquilino repositorioInquilino,
            IRepositorioInmueble repositorioInmueble,
            IRepositorioUsuario repositorioUsuario)
        {
            this.repositorio = repositorio;
            this.repositorioReserva = repositorioReserva;
            this.repositorioInquilino = repositorioInquilino;
            this.repositorioInmueble = repositorioInmueble;
            this.repositorioUsuario = repositorioUsuario;
        }

        public IActionResult Index(
            int? idReserva,
            int pagina = 1,
            string? buscar = null)
        {
            const int tamanoPagina = 10;
            pagina = Math.Max(pagina, 1);

            if (idReserva.HasValue)
            {
                Reserva? reserva = repositorioReserva.ObtenerPorId(idReserva.Value);
                if (reserva == null)
                {
                    return NotFound();
                }

                ViewBag.ReservaSeleccionada = reserva;
            }

            var lista = repositorio.ObtenerLista(
                pagina,
                tamanoPagina,
                buscar,
                idReserva,
                out int totalRegistros);
            CargarDatosListado(lista);
            ViewBag.Pagina = pagina;
            ViewBag.TotalPaginas = (int)Math.Ceiling(totalRegistros / (double)tamanoPagina);
            ViewBag.Buscar = buscar;
            ViewBag.IdReserva = idReserva;
            return View(lista);
        }

        public IActionResult Details(int id)
        {
            Pago? pago = repositorio.ObtenerPorId(id);
            if (pago == null)
            {
                return NotFound();
            }

            CargarDetalle(pago);
            return View(pago);
        }

        public IActionResult Create(int? idReserva)
        {
            var pago = new Pago
            {
                IdReserva = idReserva ?? 0,
                FechaPago = DateTime.Now
            };

            if (idReserva.HasValue)
            {
                Reserva? reserva = repositorioReserva.ObtenerPorId(idReserva.Value);
                if (reserva == null)
                {
                    return NotFound();
                }

                ViewBag.ReservaActual = reserva;
                ConfigurarPagoInicial(pago, reserva);
            }

            return View(pago);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Pago pago)
        {
            Reserva? reserva = repositorioReserva.ObtenerPorId(pago.IdReserva);
            if (reserva == null)
            {
                ModelState.AddModelError(nameof(pago.IdReserva), "La reserva seleccionada no existe");
            }
            else
            {
                decimal? montoMinimo = ObtenerMontoMinimoInicial(reserva);
                if (montoMinimo.HasValue && pago.Monto < montoMinimo.Value)
                {
                    ModelState.AddModelError(
                        nameof(pago.Monto),
                        $"El primer pago debe ser de al menos {montoMinimo.Value:C}");
                }
            }

            if (pago.FechaPago == default)
            {
                ModelState.AddModelError(nameof(pago.FechaPago), "La fecha de pago es obligatoria");
            }

            if (!ModelState.IsValid)
            {
                if (reserva != null)
                {
                    ViewBag.ReservaActual = reserva;
                    ConfigurarPagoInicial(pago, reserva, completarValores: false);
                }
                return View(pago);
            }

            pago.Concepto = pago.Concepto.Trim();
            pago.Estado = Pago.EstadoActivo;
            pago.IdUsuarioCreacion = ObtenerIdUsuarioActual();
            pago.IdUsuarioAnulacion = null;
            repositorio.Alta(pago);

            TempData["Mensaje"] = "Pago registrado correctamente";
            return RedirectToAction(nameof(Index), new { idReserva = pago.IdReserva });
        }

        public IActionResult Edit(int id)
        {
            Pago? pago = repositorio.ObtenerPorId(id);
            if (pago == null)
            {
                return NotFound();
            }

            if (pago.Estado == Pago.EstadoAnulado)
            {
                TempData["Error"] = "No se puede editar un pago anulado";
                return RedirectToAction(nameof(Details), new { id });
            }

            return View(new PagoEditarView
            {
                IdPago = pago.IdPago,
                Concepto = pago.Concepto
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, PagoEditarView modelo)
        {
            if (id != modelo.IdPago)
            {
                return BadRequest();
            }

            Pago? pago = repositorio.ObtenerPorId(id);
            if (pago == null)
            {
                return NotFound();
            }

            if (pago.Estado == Pago.EstadoAnulado)
            {
                TempData["Error"] = "No se puede editar un pago anulado";
                return RedirectToAction(nameof(Details), new { id });
            }

            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            pago.Concepto = modelo.Concepto.Trim();
            repositorio.Modificacion(pago);
            TempData["Mensaje"] = "Concepto del pago actualizado correctamente";
            return RedirectToAction(nameof(Details), new { id });
        }

        [Authorize(Policy = Usuario.RolAdministrador)]
        public IActionResult Anular(int id)
        {
            Pago? pago = repositorio.ObtenerPorId(id);
            if (pago == null)
            {
                return NotFound();
            }

            if (pago.Estado == Pago.EstadoAnulado)
            {
                TempData["Error"] = "El pago ya se encuentra anulado";
                return RedirectToAction(nameof(Details), new { id });
            }

            CargarDetalle(pago);
            return View(pago);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Usuario.RolAdministrador)]
        public IActionResult AnularConfirmado(int id)
        {
            Pago? pago = repositorio.ObtenerPorId(id);
            if (pago == null)
            {
                return NotFound();
            }

            int filas = repositorio.Anular(id, ObtenerIdUsuarioActual());
            TempData[filas > 0 ? "Mensaje" : "Error"] = filas > 0
                ? "Pago anulado correctamente"
                : "El pago ya estaba anulado";

            return RedirectToAction(nameof(Index), new { idReserva = pago.IdReserva });
        }

        private int ObtenerIdUsuarioActual()
        {
            string? valor = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(valor, out int id) ? id : 0;
        }

        private void CargarDatosListado(IList<Pago> pagos)
        {
            var reservas = pagos
                .Select(p => repositorioReserva.ObtenerPorId(p.IdReserva))
                .Where(r => r != null)
                .ToList();
            ViewBag.Reservas = reservas;
            ViewBag.Inquilinos = reservas
                .Select(r => repositorioInquilino.ObtenerPorId(r!.IdInquilino))
                .Where(i => i != null)
                .ToList();
            ViewBag.Inmuebles = reservas
                .Select(r => repositorioInmueble.ObtenerPorId(r!.IdInmueble))
                .Where(i => i != null)
                .ToList();
        }

        private void CargarDetalle(Pago pago)
        {
            ViewBag.Reserva = repositorioReserva.ObtenerPorId(pago.IdReserva);
            if (User.IsInRole(Usuario.RolAdministrador))
            {
                ViewBag.Usuarios = repositorioUsuario.ObtenerLista();
            }
        }

        private void ConfigurarPagoInicial(
            Pago pago,
            Reserva reserva,
            bool completarValores = true)
        {
            decimal? montoMinimo = ObtenerMontoMinimoInicial(reserva);
            if (!montoMinimo.HasValue)
            {
                return;
            }

            Inmueble? inmueble = repositorioInmueble.ObtenerPorId(reserva.IdInmueble);
            ViewBag.MontoMinimo = montoMinimo.Value;
            ViewBag.PorcentajeReserva = inmueble?.PorcentajeReserva;

            if (completarValores)
            {
                pago.Concepto = "Seña inicial";
                pago.Monto = montoMinimo.Value;
            }
        }

        private decimal? ObtenerMontoMinimoInicial(Reserva reserva)
        {
            bool tienePagosActivos = repositorio.ObtenerPorReserva(reserva.IdReserva)
                .Any(p => p.Estado == Pago.EstadoActivo);
            if (tienePagosActivos)
            {
                return null;
            }

            Inmueble? inmueble = repositorioInmueble.ObtenerPorId(reserva.IdInmueble);
            if (inmueble == null)
            {
                return null;
            }

            int cantidadDias = (reserva.FechaFinOriginal.Date - reserva.FechaInicio.Date).Days;
            decimal totalReserva = cantidadDias * reserva.MontoDia;
            return Math.Round(totalReserva * inmueble.PorcentajeReserva / 100m, 2);
        }
    }
}
