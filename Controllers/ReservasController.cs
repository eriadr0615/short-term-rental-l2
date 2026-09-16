using System.Security.Claims;
using Inmobiliaria.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inmobiliaria.Controllers
{
    [Authorize]
    public class ReservasController : Controller
    {
        private readonly IRepositorioReserva repositorio;
        private readonly IRepositorioInquilino repositorioInquilino;
        private readonly IRepositorioInmueble repositorioInmueble;
        private readonly IRepositorioPago repositorioPago;
        private readonly IRepositorioUsuario repositorioUsuario;

        public ReservasController(
            IRepositorioReserva repositorio,
            IRepositorioInquilino repositorioInquilino,
            IRepositorioInmueble repositorioInmueble,
            IRepositorioPago repositorioPago,
            IRepositorioUsuario repositorioUsuario)
        {
            this.repositorio = repositorio;
            this.repositorioInquilino = repositorioInquilino;
            this.repositorioInmueble = repositorioInmueble;
            this.repositorioPago = repositorioPago;
            this.repositorioUsuario = repositorioUsuario;
        }

        public IActionResult Index()
        {
            var lista = repositorio.ObtenerLista();
            ViewBag.Inquilinos = repositorioInquilino.ObtenerLista();
            ViewBag.Inmuebles = repositorioInmueble.ObtenerLista();

            return View("~/Views/Reserva/Index.cshtml", lista);
        }

        public IActionResult Details(int id)
        {
            var reserva = repositorio.ObtenerPorId(id);

            if (reserva == null)
            {
                return NotFound();
            }

            ViewBag.Inquilino = repositorioInquilino.ObtenerPorId(reserva.IdInquilino);
            ViewBag.Inmueble = repositorioInmueble.ObtenerPorId(reserva.IdInmueble);
            if (User.IsInRole(Usuario.RolAdministrador))
            {
                ViewBag.Usuarios = repositorioUsuario.ObtenerLista();
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
            if (reserva.FechaFinOriginal <= reserva.FechaInicio)
            {
                ModelState.AddModelError(
                    nameof(Reserva.FechaFinOriginal),
                    "Che, el día de finalización debe ser posterior al día de inicio.");
            }

            if (ModelState.IsValid && repositorio.ExisteSuperposicion(
                    reserva.IdInmueble,
                    reserva.FechaInicio,
                    reserva.FechaFinOriginal,
                    null))
            {
                ModelState.AddModelError(
                    nameof(Reserva.IdInmueble),
                    "El inmueble ya tiene una reserva en ese período.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Inquilinos = repositorioInquilino.ObtenerLista();
                ViewBag.Inmuebles = repositorioInmueble.ObtenerLista();

                return View("~/Views/Reserva/Create.cshtml", reserva);
            }

            reserva.IdUsuarioCreacion = ObtenerIdUsuarioActual();
            repositorio.Alta(reserva);
            return RedirectToAction("Create", "Pagos", new { idReserva = reserva.IdReserva });
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

            Reserva? reservaGuardada = repositorio.ObtenerPorId(id);
            if (reservaGuardada == null)
            {
                return NotFound();
            }

            reserva.FechaFinalizacionAnticipada = reservaGuardada.FechaFinalizacionAnticipada;
            reserva.IdUsuarioCreacion = reservaGuardada.IdUsuarioCreacion;
            reserva.IdUsuarioFinalizacion = reservaGuardada.IdUsuarioFinalizacion;
            reserva.IdReservaOrigen = reservaGuardada.IdReservaOrigen;

            if (reserva.FechaFinOriginal <= reserva.FechaInicio)
            {
                ModelState.AddModelError(
                    nameof(Reserva.FechaFinOriginal),
                    "Che, el día de finalización debe ser posterior al día de inicio.");
            }

            if (ModelState.IsValid && repositorio.ExisteSuperposicion(
                    reserva.IdInmueble,
                    reserva.FechaInicio,
                    reserva.FechaFinOriginal,
                    reserva.IdReserva))
            {
                ModelState.AddModelError(
                    nameof(Reserva.IdInmueble),
                    "El inmueble ya tiene una reserva en ese período.");
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

        public IActionResult Finalizar(int id)
        {
            Reserva? reserva = repositorio.ObtenerPorId(id);
            if (reserva == null)
            {
                return NotFound();
            }

            if (reserva.FechaFinalizacionAnticipada.HasValue)
            {
                TempData["Error"] = "La reserva ya fue finalizada anticipadamente";
                return RedirectToAction(nameof(Details), new { id });
            }

            CargarDatosFinalizacion(reserva);
            return View("~/Views/Reserva/Finalizar.cshtml", new FinalizarReservaView
            {
                IdReserva = reserva.IdReserva,
                FechaFinalizacion = DateTime.Today
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Finalizar(FinalizarReservaView modelo, string accion)
        {
            Reserva? reserva = repositorio.ObtenerPorId(modelo.IdReserva);
            if (reserva == null)
            {
                return NotFound();
            }

            if (reserva.FechaFinalizacionAnticipada.HasValue)
            {
                TempData["Error"] = "La reserva ya fue finalizada anticipadamente";
                return RedirectToAction(nameof(Details), new { id = reserva.IdReserva });
            }

            ValidarFechaFinalizacion(reserva, modelo);
            if (ModelState.IsValid)
            {
                CalcularMulta(reserva, modelo);
            }

            if (!ModelState.IsValid || accion == "calcular")
            {
                CargarDatosFinalizacion(reserva);
                return View("~/Views/Reserva/Finalizar.cshtml", modelo);
            }

            var pagoMulta = new Pago
            {
                IdReserva = reserva.IdReserva,
                Concepto = "Multa por finalización anticipada",
                FechaPago = DateTime.Now,
                Monto = modelo.MultaCalculada,
                Estado = Pago.EstadoActivo,
                IdUsuarioCreacion = ObtenerIdUsuarioActual()
            };

            repositorioPago.Alta(pagoMulta);
            int filas = repositorio.Finalizar(
                reserva.IdReserva,
                modelo.FechaFinalizacion,
                ObtenerIdUsuarioActual());

            if (filas == 0)
            {
                TempData["Error"] = "No fue posible finalizar la reserva";
                return RedirectToAction(nameof(Details), new { id = reserva.IdReserva });
            }

            TempData["Mensaje"] = $"Reserva finalizada. Multa registrada: {modelo.MultaCalculada:C}";
            return RedirectToAction(nameof(Details), new { id = reserva.IdReserva });
        }

        public IActionResult Renovar(int id)
        {
            Reserva? reserva = repositorio.ObtenerPorId(id);
            if (reserva == null)
            {
                return NotFound();
            }

            DateTime inicioSugerido = reserva.FechaFinalizacionAnticipada
                ?? reserva.FechaFinOriginal;
            CargarDatosRenovacion(reserva);
            return View("~/Views/Reserva/Renovar.cshtml", new RenovarReservaView
            {
                IdReservaOrigen = reserva.IdReserva,
                FechaInicio = inicioSugerido,
                FechaFin = inicioSugerido.AddDays(1),
                MontoDia = reserva.MontoDia
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Renovar(RenovarReservaView modelo)
        {
            Reserva? reservaOrigen = repositorio.ObtenerPorId(modelo.IdReservaOrigen);
            if (reservaOrigen == null)
            {
                return NotFound();
            }

            DateTime finEfectivo = reservaOrigen.FechaFinalizacionAnticipada
                ?? reservaOrigen.FechaFinOriginal;

            if (modelo.FechaInicio < finEfectivo)
            {
                ModelState.AddModelError(
                    nameof(modelo.FechaInicio),
                    $"La renovación debe comenzar el {finEfectivo:dd/MM/yyyy} o después");
            }

            if (modelo.FechaFin <= modelo.FechaInicio)
            {
                ModelState.AddModelError(
                    nameof(modelo.FechaFin),
                    "La fecha de finalización debe ser posterior a la fecha de inicio");
            }

            if (ModelState.IsValid && repositorio.ExisteSuperposicion(
                    reservaOrigen.IdInmueble,
                    modelo.FechaInicio,
                    modelo.FechaFin,
                    null))
            {
                ModelState.AddModelError(
                    nameof(modelo.FechaInicio),
                    "El inmueble ya tiene una reserva en ese período");
            }

            if (!ModelState.IsValid)
            {
                CargarDatosRenovacion(reservaOrigen);
                return View("~/Views/Reserva/Renovar.cshtml", modelo);
            }

            var nuevaReserva = new Reserva
            {
                IdInquilino = reservaOrigen.IdInquilino,
                IdInmueble = reservaOrigen.IdInmueble,
                FechaInicio = modelo.FechaInicio,
                FechaFinOriginal = modelo.FechaFin,
                MontoDia = modelo.MontoDia,
                IdUsuarioCreacion = ObtenerIdUsuarioActual(),
                IdReservaOrigen = reservaOrigen.IdReserva
            };

            repositorio.Alta(nuevaReserva);
            TempData["Mensaje"] = $"Renovación creada como reserva #{nuevaReserva.IdReserva}";
            return RedirectToAction("Create", "Pagos", new { idReserva = nuevaReserva.IdReserva });
        }

        [Authorize(Policy = Usuario.RolAdministrador)]
        public IActionResult Delete(int id)
        {
            var reserva = repositorio.ObtenerPorId(id);

            if (reserva == null)
            {
                return NotFound();
            }

            ViewBag.Inquilino = repositorioInquilino.ObtenerPorId(reserva.IdInquilino);
            ViewBag.Inmueble = repositorioInmueble.ObtenerPorId(reserva.IdInmueble);

            return View("~/Views/Reserva/Delete.cshtml", reserva);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Usuario.RolAdministrador)]
        public IActionResult DeleteConfirmed(int id)
        {
            repositorio.Baja(id);
            return RedirectToAction(nameof(Index));
        }

        private int ObtenerIdUsuarioActual()
        {
            string? valor = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(valor, out int id) ? id : 0;
        }

        private void ValidarFechaFinalizacion(
            Reserva reserva,
            FinalizarReservaView modelo)
        {
            if (modelo.FechaFinalizacion < reserva.FechaInicio ||
                modelo.FechaFinalizacion >= reserva.FechaFinOriginal)
            {
                ModelState.AddModelError(
                    nameof(modelo.FechaFinalizacion),
                    $"La fecha debe estar entre {reserva.FechaInicio:dd/MM/yyyy} y el día anterior a {reserva.FechaFinOriginal:dd/MM/yyyy}");
            }
        }

        private static void CalcularMulta(
            Reserva reserva,
            FinalizarReservaView modelo)
        {
            int diasTotales = (reserva.FechaFinOriginal.Date - reserva.FechaInicio.Date).Days;
            int diasCumplidos = (modelo.FechaFinalizacion.Date - reserva.FechaInicio.Date).Days;
            int diasRestantes = (reserva.FechaFinOriginal.Date - modelo.FechaFinalizacion.Date).Days;
            int porcentaje = diasCumplidos * 2 < diasTotales ? 50 : 25;

            modelo.DiasRestantes = diasRestantes;
            modelo.PorcentajeMulta = porcentaje;
            modelo.MultaCalculada = Math.Round(
                diasRestantes * reserva.MontoDia * porcentaje / 100m,
                2);
            modelo.Calculada = true;
        }

        private void CargarDatosFinalizacion(Reserva reserva)
        {
            ViewBag.Reserva = reserva;
            ViewBag.Inquilino = repositorioInquilino.ObtenerPorId(reserva.IdInquilino);
            ViewBag.Inmueble = repositorioInmueble.ObtenerPorId(reserva.IdInmueble);
        }

        private void CargarDatosRenovacion(Reserva reserva)
        {
            ViewBag.ReservaOrigen = reserva;
            ViewBag.Inquilino = repositorioInquilino.ObtenerPorId(reserva.IdInquilino);
            ViewBag.Inmueble = repositorioInmueble.ObtenerPorId(reserva.IdInmueble);
        }
    }
}
