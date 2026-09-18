using System.Security.Claims;
using Inmobiliaria.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;

namespace Inmobiliaria.Controllers
{
    [Authorize]
    public class ReservasController : Controller
    {
        private readonly IRepositorioReserva repositorio;
        private readonly IRepositorioInquilino repositorioInquilino;
        private readonly IRepositorioInmueble repositorioInmueble;
        private readonly IRepositorioUsuario repositorioUsuario;

        public ReservasController(
            IRepositorioReserva repositorio,
            IRepositorioInquilino repositorioInquilino,
            IRepositorioInmueble repositorioInmueble,
            IRepositorioUsuario repositorioUsuario)
        {
            this.repositorio = repositorio;
            this.repositorioInquilino = repositorioInquilino;
            this.repositorioInmueble = repositorioInmueble;
            this.repositorioUsuario = repositorioUsuario;
        }

        public IActionResult Index(int pagina = 1, string? buscar = null)
        {
            const int tamanoPagina = 10;
            pagina = Math.Max(pagina, 1);
            var lista = repositorio.ObtenerLista(
                pagina, tamanoPagina, buscar, out int totalRegistros);
            ViewBag.Inquilinos = lista
                .Select(r => repositorioInquilino.ObtenerPorId(r.IdInquilino))
                .Where(i => i != null)
                .ToList();
            ViewBag.Inmuebles = lista
                .Select(r => repositorioInmueble.ObtenerPorId(r.IdInmueble))
                .Where(i => i != null)
                .ToList();
            ViewBag.Pagina = pagina;
            ViewBag.TotalPaginas = (int)Math.Ceiling(totalRegistros / (double)tamanoPagina);
            ViewBag.Buscar = buscar;

            return View("~/Views/Reserva/Index.cshtml", lista);
        }

        [HttpGet]
        public IActionResult Buscar(string q)
        {
            if (string.IsNullOrWhiteSpace(q)
                || (q.Trim().Length < 2 && !int.TryParse(q.Trim(), out _)))
            {
                return Json(Array.Empty<object>());
            }

            var resultado = repositorio.Buscar(q, 10)
                .Select(r =>
                {
                    var inquilino = repositorioInquilino.ObtenerPorId(r.IdInquilino);
                    var inmueble = repositorioInmueble.ObtenerPorId(r.IdInmueble);
                    return new
                    {
                        id = r.IdReserva,
                        texto = $"#{r.IdReserva} - {inquilino?.Nombre} {inquilino?.Apellido} - {inmueble?.DireccionInmueble}"
                    };
                });
            return Json(resultado);
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
            return View("~/Views/Reserva/Create.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("IdInquilino,IdInmueble,FechaInicio,FechaFinOriginal,MontoDia")] Reserva reserva)
        {
            ValidarReserva(reserva);

            if (!ModelState.IsValid)
            {
                CargarSeleccionesReserva(reserva);

                return View("~/Views/Reserva/Create.cshtml", reserva);
            }

            return PrepararConfirmacion(reserva);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Confirmar(ConfirmarReservaView modelo)
        {
            // Solo se toman los datos del alquiler, nunca la auditoría enviada por el formulario.
            var reserva = new Reserva
            {
                IdInquilino = modelo.Reserva.IdInquilino,
                IdInmueble = modelo.Reserva.IdInmueble,
                FechaInicio = modelo.Reserva.FechaInicio,
                FechaFinOriginal = modelo.Reserva.FechaFinOriginal,
                MontoDia = modelo.Reserva.MontoDia,
                IdReservaOrigen = modelo.Reserva.IdReservaOrigen,
                IdUsuarioCreacion = ObtenerIdUsuarioActual()
            };
            modelo.Reserva = reserva;
            ValidarReserva(reserva, prefijo: "Reserva.");
            CargarConfirmacion(modelo);

            if (modelo.MontoPago < modelo.MontoMinimo)
                ModelState.AddModelError(nameof(modelo.MontoPago), $"Debe registrar al menos {modelo.MontoMinimo:C}");
            if (modelo.MontoPago != Math.Round(modelo.MontoPago, 2))
                ModelState.AddModelError(nameof(modelo.MontoPago), "El importe admite hasta dos decimales");

            if (ModelState.IsValid)
            {
                Pago? pago = modelo.MontoPago > 0 ? new Pago
                {
                    Concepto = modelo.Concepto.Trim(),
                    FechaPago = DateTime.Now,
                    Monto = modelo.MontoPago,
                    IdUsuarioCreacion = ObtenerIdUsuarioActual()
                } : null;
                try
                {
                    repositorio.AltaConPago(reserva, pago);
                    TempData["Mensaje"] = pago == null
                        ? "Reserva creada. El inmueble no exige una seña inicial."
                        : "Reserva y pago inicial registrados correctamente";
                    return RedirectToAction(nameof(Details), new { id = reserva.IdReserva });
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
                catch (MySqlException)
                {
                    ModelState.AddModelError("", "No se pudo guardar la reserva con su pago. No se guardó ninguno de los dos.");
                }
            }
            return View("~/Views/Reserva/Confirmar.cshtml", modelo);
        }

        public IActionResult Edit(int id)
        {
            var reserva = repositorio.ObtenerPorId(id);

            if (reserva == null)
            {
                return NotFound();
            }

            if (reserva.FechaFinalizacionAnticipada.HasValue)
                return ReservaNoEditable(reserva.IdReserva);
            CargarSeleccionesReserva(reserva);
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
            if (reservaGuardada.FechaFinalizacionAnticipada.HasValue)
                return ReservaNoEditable(id);
            reserva.IdUsuarioCreacion = reservaGuardada.IdUsuarioCreacion;
            reserva.IdUsuarioFinalizacion = reservaGuardada.IdUsuarioFinalizacion;
            reserva.IdReservaOrigen = reservaGuardada.IdReservaOrigen;

            ValidarReserva(reserva, reservaGuardada);

            if (!ModelState.IsValid)
            {
                CargarSeleccionesReserva(reserva);

                return View("~/Views/Reserva/Edit.cshtml", reserva);
            }

            try
            {
                if (repositorio.Modificacion(reserva) > 0)
                    return RedirectToAction(nameof(Index));
                ModelState.AddModelError("", "La reserva ya no se puede editar. Volvé a consultar su detalle.");
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            catch (MySqlException)
            {
                ModelState.AddModelError("", "No se pudo modificar la reserva. No se guardaron los cambios.");
            }
            CargarSeleccionesReserva(reserva);
            return View("~/Views/Reserva/Edit.cshtml", reserva);
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
            if (accion != "calcular" && accion != "confirmar")
                return BadRequest();

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

            decimal multaMostrada = modelo.MultaCalculada;
            DateTime? fechaCalculada = modelo.FechaCalculada;
            bool calculada = modelo.Calculada;
            modelo.Calculada = false;
            ValidarFechaFinalizacion(reserva, modelo);
            if (ModelState.IsValid)
            {
                CalcularMulta(reserva, modelo);
                if (accion == "confirmar" && (!calculada ||
                    fechaCalculada != modelo.FechaFinalizacion || multaMostrada != modelo.MultaCalculada))
                    ModelState.AddModelError("", "La fecha o el importe cambiaron. Revisá la multa recalculada y confirmá nuevamente.");
            }

            // Razor debe mostrar el nuevo cálculo, no los valores ocultos del POST anterior.
            foreach (string campo in new[] { "MultaCalculada", "PorcentajeMulta", "DiasRestantes", "Calculada", "FechaCalculada" })
                ModelState.Remove(campo);

            if (!ModelState.IsValid || !modelo.Calculada || accion == "calcular")
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

            try
            {
                int filas = repositorio.FinalizarConPago(reserva, modelo.FechaFinalizacion, pagoMulta);
                if (filas > 0)
                {
                    TempData["Mensaje"] = $"Reserva finalizada. Multa registrada: {modelo.MultaCalculada:C}";
                    return RedirectToAction(nameof(Details), new { id = reserva.IdReserva });
                }
                ModelState.AddModelError("", "La reserva cambió o ya fue finalizada. Volvé a consultar su detalle. No se registró otro pago.");
            }
            catch (MySqlException)
            {
                ModelState.AddModelError("", "No se pudo registrar el pago y finalizar la reserva. No se guardó ninguna de las dos operaciones.");
            }
            CargarDatosFinalizacion(reserva);
            return View("~/Views/Reserva/Finalizar.cshtml", modelo);
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

            if (modelo.FechaInicio.Year < 1000 || modelo.FechaFin.Year < 1000)
                ModelState.AddModelError("", "Debe completar ambas fechas con valores válidos");
            if (modelo.MontoDia <= 0 || modelo.MontoDia != Math.Round(modelo.MontoDia, 2))
                ModelState.AddModelError(nameof(modelo.MontoDia), "El monto diario debe ser positivo y tener hasta dos decimales");

            Inmueble? inmueble = repositorioInmueble.ObtenerPorId(reservaOrigen.IdInmueble);
            if (inmueble == null || !inmueble.Disponible)
                ModelState.AddModelError("", "El inmueble está suspendido o no existe; no se puede crear una renovación");

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

            return PrepararConfirmacion(nuevaReserva);
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
            var reserva = repositorio.ObtenerPorId(id);
            if (reserva == null) return NotFound();
            try
            {
                repositorio.Baja(id);
                return RedirectToAction(nameof(Index));
            }
            catch (MySqlException ex) when (ex.Number == 1451)
            {
                ModelState.AddModelError("", "No se puede eliminar esta reserva porque tiene pagos o renovaciones asociados. Los pagos anulados también forman parte de su historial.");
            }
            catch (MySqlException)
            {
                ModelState.AddModelError("", "No se pudo eliminar la reserva. Intentá nuevamente.");
            }
            ViewBag.Inquilino = repositorioInquilino.ObtenerPorId(reserva.IdInquilino);
            ViewBag.Inmueble = repositorioInmueble.ObtenerPorId(reserva.IdInmueble);
            return View("~/Views/Reserva/Delete.cshtml", reserva);
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
            if (reserva.MontoDia <= 0 || reserva.FechaFinOriginal <= reserva.FechaInicio)
                ModelState.AddModelError("", "La reserva tiene fechas o monto diario inválidos. Deben corregirse antes de finalizarla.");
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
            modelo.FechaCalculada = modelo.FechaFinalizacion;
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

        private IActionResult ReservaNoEditable(int id)
        {
            TempData["Error"] = "No se puede editar una reserva finalizada: deben conservarse los datos originales para reconstruir la multa";
            return RedirectToAction(nameof(Details), new { id });
        }

        private void ValidarReserva(Reserva reserva, Reserva? anterior = null, string prefijo = "")
        {
            if (reserva.FechaInicio.Year < 1000)
                ModelState.AddModelError(prefijo + nameof(reserva.FechaInicio), "La fecha de inicio es obligatoria y debe ser válida");
            if (reserva.FechaFinOriginal.Year < 1000 || reserva.FechaFinOriginal.Date <= reserva.FechaInicio.Date)
                ModelState.AddModelError(prefijo + nameof(reserva.FechaFinOriginal), "La finalización debe ser posterior al inicio");
            if (reserva.MontoDia <= 0 || reserva.MontoDia > 9999999999.99m || reserva.MontoDia != Math.Round(reserva.MontoDia, 2))
                ModelState.AddModelError(prefijo + nameof(reserva.MontoDia), "El monto diario debe ser positivo y tener hasta dos decimales");
            if (repositorioInquilino.ObtenerPorId(reserva.IdInquilino) == null)
                ModelState.AddModelError(prefijo + nameof(reserva.IdInquilino), "El inquilino seleccionado no existe");

            Inmueble? inmueble = repositorioInmueble.ObtenerPorId(reserva.IdInmueble);
            if (inmueble == null)
                ModelState.AddModelError(prefijo + nameof(reserva.IdInmueble), "El inmueble seleccionado no existe");
            else if (!inmueble.Disponible && (anterior == null || anterior.IdInmueble != reserva.IdInmueble))
                ModelState.AddModelError(prefijo + nameof(reserva.IdInmueble), "El inmueble está suspendido y no admite nuevas reservas");

            if (reserva.IdReservaOrigen.HasValue)
            {
                Reserva? origen = repositorio.ObtenerPorId(reserva.IdReservaOrigen.Value);
                if (origen == null || origen.IdInmueble != reserva.IdInmueble || origen.IdInquilino != reserva.IdInquilino ||
                    reserva.FechaInicio < (origen.FechaFinalizacionAnticipada ?? origen.FechaFinOriginal))
                    ModelState.AddModelError("", "La renovación debe conservar el inquilino y el inmueble, y comenzar al terminar la reserva original o después");
            }

            if (ModelState.IsValid && repositorio.ExisteSuperposicion(reserva.IdInmueble,
                reserva.FechaInicio, reserva.FechaFinOriginal, anterior?.IdReserva))
                ModelState.AddModelError(prefijo + nameof(reserva.IdInmueble), "El inmueble ya tiene una reserva en ese período");
        }

        private IActionResult PrepararConfirmacion(Reserva reserva)
        {
            var modelo = new ConfirmarReservaView { Reserva = reserva };
            CargarConfirmacion(modelo);
            modelo.MontoPago = modelo.MontoMinimo;
            return View("~/Views/Reserva/Confirmar.cshtml", modelo);
        }

        private void CargarConfirmacion(ConfirmarReservaView modelo)
        {
            Inmueble? inmueble = repositorioInmueble.ObtenerPorId(modelo.Reserva.IdInmueble);
            ViewBag.Inmueble = inmueble;
            ViewBag.Inquilino = repositorioInquilino.ObtenerPorId(modelo.Reserva.IdInquilino);
            modelo.PorcentajeReserva = inmueble?.PorcentajeReserva ?? 0;
            int dias = (modelo.Reserva.FechaFinOriginal.Date - modelo.Reserva.FechaInicio.Date).Days;
            modelo.MontoMinimo = dias > 0 && modelo.Reserva.MontoDia > 0 && modelo.Reserva.MontoDia <= 9999999999.99m &&
                modelo.PorcentajeReserva >= 0 && modelo.PorcentajeReserva <= 100
                ? Math.Round(dias * modelo.Reserva.MontoDia * modelo.PorcentajeReserva / 100m, 2)
                : 0;
            if (inmueble != null && (inmueble.PorcentajeReserva < 0 || inmueble.PorcentajeReserva > 100))
                ModelState.AddModelError("", "El inmueble tiene un porcentaje de reserva inválido. Corregilo antes de reservar.");
        }

        private void CargarSeleccionesReserva(Reserva reserva)
        {
            ViewBag.InquilinoActual = repositorioInquilino.ObtenerPorId(reserva.IdInquilino);
            ViewBag.InmuebleActual = repositorioInmueble.ObtenerPorId(reserva.IdInmueble);
        }
    }
}
