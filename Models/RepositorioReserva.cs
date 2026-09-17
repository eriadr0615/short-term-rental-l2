using MySqlConnector;

namespace Inmobiliaria.Models
{
    public class RepositorioReserva : RepositorioBase, IRepositorioReserva
    {
        public RepositorioReserva(IConfiguration configuration)
            : base(configuration)
        {
        }

        public int Alta(Reserva reserva)
        {
            return AltaConPago(reserva, null);
        }

        public int AltaConPago(Reserva reserva, Pago? pagoInicial)
        {
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);
            try
            {
                Inmueble inmueble = BloquearInmueble(reserva.IdInmueble, connection, transaction);
                ValidarDatos(reserva);
                if (!inmueble.Disponible)
                    throw new InvalidOperationException("El inmueble está suspendido y no admite nuevas reservas");
                if (inmueble.PorcentajeReserva < 0 || inmueble.PorcentajeReserva > 100)
                    throw new InvalidOperationException("El porcentaje de reserva del inmueble debe estar entre 0 y 100");
                ValidarOrigen(reserva, connection, transaction);
                if (ExisteSuperposicion(reserva.IdInmueble, reserva.FechaInicio,
                    reserva.FechaFinOriginal, null, connection, transaction))
                    throw new InvalidOperationException("El inmueble ya tiene una reserva en ese período");

                int dias = (reserva.FechaFinOriginal.Date - reserva.FechaInicio.Date).Days;
                decimal minimo = Math.Round(dias * reserva.MontoDia * inmueble.PorcentajeReserva / 100m, 2);
                if ((pagoInicial?.Monto ?? 0) < minimo)
                    throw new InvalidOperationException($"La seña requerida es de al menos {minimo:C}. Revisá el importe antes de confirmar.");

                string sql = @"INSERT INTO Reserva
                    (id_inquilino, id_inmueble, fecha_inicio, fecha_fin_original,
                     monto_dia, id_usuario_creacion, id_reserva_origen)
                    VALUES (@idInquilino, @idInmueble, @inicio, @fin, @monto, @usuario, @origen);
                    SELECT LAST_INSERT_ID();";
                using var command = new MySqlCommand(sql, connection, transaction);
                command.Parameters.AddWithValue("@idInquilino", reserva.IdInquilino);
                command.Parameters.AddWithValue("@idInmueble", reserva.IdInmueble);
                command.Parameters.AddWithValue("@inicio", reserva.FechaInicio.Date);
                command.Parameters.AddWithValue("@fin", reserva.FechaFinOriginal.Date);
                command.Parameters.AddWithValue("@monto", reserva.MontoDia);
                command.Parameters.AddWithValue("@usuario", reserva.IdUsuarioCreacion);
                command.Parameters.AddWithValue("@origen", (object?)reserva.IdReservaOrigen ?? DBNull.Value);
                int id = Convert.ToInt32(command.ExecuteScalar());
                if (pagoInicial != null)
                {
                    pagoInicial.IdReserva = id;
                    RepositorioPago.Insertar(pagoInicial, connection, transaction);
                }

                // Reserva y pago se confirman juntos; si algo falla, se deshacen ambos.
                transaction.Commit();
                reserva.IdReserva = id;
                return id;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public int Baja(int id)
        {
            int res = -1;

            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"DELETE FROM Reserva
                               WHERE id_reserva = @id";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);

                    connection.Open();

                    res = command.ExecuteNonQuery();
                }
            }

            return res;
        }

        public int Modificacion(Reserva reserva)
        {
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);
            try
            {
                Inmueble inmueble = BloquearInmueble(reserva.IdInmueble, connection, transaction);
                ValidarDatos(reserva);
                using (var anterior = new MySqlCommand(
                    "SELECT id_inmueble, fecha_finalizacion_anticipada FROM Reserva WHERE id_reserva = @id FOR UPDATE",
                    connection, transaction))
                {
                    anterior.Parameters.AddWithValue("@id", reserva.IdReserva);
                    using var reader = anterior.ExecuteReader();
                    if (!reader.Read() || reader["fecha_finalizacion_anticipada"] != DBNull.Value)
                    {
                        reader.Close();
                        transaction.Rollback();
                        return 0;
                    }
                    // Suspender una oferta no invalida sus reservas existentes.
                    if (!inmueble.Disponible && Convert.ToInt32(reader["id_inmueble"]) != reserva.IdInmueble)
                        throw new InvalidOperationException("El nuevo inmueble está suspendido");
                }
                ValidarOrigen(reserva, connection, transaction);
                if (ExisteSuperposicion(reserva.IdInmueble, reserva.FechaInicio,
                    reserva.FechaFinOriginal, reserva.IdReserva, connection, transaction))
                    throw new InvalidOperationException("El inmueble ya tiene una reserva en ese período");

                string sql = @"UPDATE Reserva
                               SET id_inquilino = @idInquilino,
                                   id_inmueble = @idInmueble,
                                   fecha_inicio = @fechaInicio,
                                   fecha_fin_original = @fechaFinOriginal,
                                   monto_dia = @montoDia
                               WHERE id_reserva = @id
                                 AND fecha_finalizacion_anticipada IS NULL";

                using (var command = new MySqlCommand(sql, connection, transaction))
                {
                    command.Parameters.AddWithValue("@idInquilino", reserva.IdInquilino);
                    command.Parameters.AddWithValue("@idInmueble", reserva.IdInmueble);
                    command.Parameters.AddWithValue("@fechaInicio", reserva.FechaInicio);
                    command.Parameters.AddWithValue("@fechaFinOriginal", reserva.FechaFinOriginal);
                    command.Parameters.AddWithValue("@montoDia", reserva.MontoDia);
                    command.Parameters.AddWithValue("@id", reserva.IdReserva);
                    int res = command.ExecuteNonQuery();
                    transaction.Commit();
                    return res;
                }
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public IList<Reserva> ObtenerLista()
        {
            IList<Reserva> lista = new List<Reserva>();

            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"SELECT
                                id_reserva,
                                id_inquilino,
                                id_inmueble,
                                fecha_inicio,
                                fecha_fin_original,
                                monto_dia,
                                fecha_finalizacion_anticipada,
                                id_usuario_creacion,
                                id_usuario_finalizacion,
                                id_reserva_origen
                               FROM Reserva";

                using (var command = new MySqlCommand(sql, connection))
                {
                    connection.Open();

                    var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        Reserva reserva = new Reserva
                        {
                            IdReserva = Convert.ToInt32(reader["id_reserva"]),
                            IdInquilino = Convert.ToInt32(reader["id_inquilino"]),
                            IdInmueble = Convert.ToInt32(reader["id_inmueble"]),
                            FechaInicio = Convert.ToDateTime(reader["fecha_inicio"]),
                            FechaFinOriginal = Convert.ToDateTime(reader["fecha_fin_original"]),
                            MontoDia = Convert.ToDecimal(reader["monto_dia"]),
                            FechaFinalizacionAnticipada = reader["fecha_finalizacion_anticipada"] == DBNull.Value
                                ? null
                                : Convert.ToDateTime(reader["fecha_finalizacion_anticipada"]),
                            IdUsuarioCreacion = reader["id_usuario_creacion"] == DBNull.Value
                                ? null
                                : Convert.ToInt32(reader["id_usuario_creacion"]),
                            IdUsuarioFinalizacion = reader["id_usuario_finalizacion"] == DBNull.Value
                                ? null
                                : Convert.ToInt32(reader["id_usuario_finalizacion"]),
                            IdReservaOrigen = reader["id_reserva_origen"] == DBNull.Value
                                ? null
                                : Convert.ToInt32(reader["id_reserva_origen"])
                        };

                        lista.Add(reserva);
                    }
                }
            }

            return lista;
        }

        public Reserva? ObtenerPorId(int id)
        {
            Reserva? reserva = null;

            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"SELECT
                                id_reserva,
                                id_inquilino,
                                id_inmueble,
                                fecha_inicio,
                                fecha_fin_original,
                                monto_dia,
                                fecha_finalizacion_anticipada,
                                id_usuario_creacion,
                                id_usuario_finalizacion,
                                id_reserva_origen
                               FROM Reserva
                               WHERE id_reserva = @id";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);

                    connection.Open();

                    var reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        reserva = new Reserva
                        {
                            IdReserva = Convert.ToInt32(reader["id_reserva"]),
                            IdInquilino = Convert.ToInt32(reader["id_inquilino"]),
                            IdInmueble = Convert.ToInt32(reader["id_inmueble"]),
                            FechaInicio = Convert.ToDateTime(reader["fecha_inicio"]),
                            FechaFinOriginal = Convert.ToDateTime(reader["fecha_fin_original"]),
                            MontoDia = Convert.ToDecimal(reader["monto_dia"]),
                            FechaFinalizacionAnticipada = reader["fecha_finalizacion_anticipada"] == DBNull.Value
                                ? null
                                : Convert.ToDateTime(reader["fecha_finalizacion_anticipada"]),
                            IdUsuarioCreacion = reader["id_usuario_creacion"] == DBNull.Value
                                ? null
                                : Convert.ToInt32(reader["id_usuario_creacion"]),
                            IdUsuarioFinalizacion = reader["id_usuario_finalizacion"] == DBNull.Value
                                ? null
                                : Convert.ToInt32(reader["id_usuario_finalizacion"]),
                            IdReservaOrigen = reader["id_reserva_origen"] == DBNull.Value
                                ? null
                                : Convert.ToInt32(reader["id_reserva_origen"])
                        };
                    }
                }
            }

            return reserva;
        }

        public bool ExisteSuperposicion(
            int idInmueble,
            DateTime fechaInicio,
            DateTime fechaFin,
            int? idReservaExcluir)
        {
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            return ExisteSuperposicion(idInmueble, fechaInicio, fechaFin, idReservaExcluir, connection, null);
        }

        private static bool ExisteSuperposicion(int idInmueble, DateTime fechaInicio,
            DateTime fechaFin, int? idReservaExcluir, MySqlConnection connection, MySqlTransaction? transaction)
        {
            string sql = @"SELECT COUNT(*)
                               FROM Reserva
                               WHERE id_inmueble = @idInmueble
                                 AND fecha_inicio < @fechaFin
                                 AND COALESCE(fecha_finalizacion_anticipada, fecha_fin_original) > @fechaInicio
                                 AND (@idReservaExcluir IS NULL
                                      OR id_reserva <> @idReservaExcluir)";

            using (var command = new MySqlCommand(sql, connection, transaction))
            {
                command.Parameters.AddWithValue("@idInmueble", idInmueble);
                command.Parameters.AddWithValue("@fechaInicio", fechaInicio);
                command.Parameters.AddWithValue("@fechaFin", fechaFin);
                command.Parameters.AddWithValue(
                    "@idReservaExcluir",
                    (object?)idReservaExcluir ?? DBNull.Value);

                return Convert.ToInt32(command.ExecuteScalar()) > 0;
            }
        }

        public int FinalizarConPago(Reserva reserva, DateTime fechaFinalizacion, Pago pagoMulta)
        {
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();
            try
            {
                string sql = @"UPDATE Reserva
                           SET fecha_finalizacion_anticipada = @fechaFinalizacion,
                               id_usuario_finalizacion = @idUsuarioFinalizacion
                           WHERE id_reserva = @idReserva
                             AND fecha_finalizacion_anticipada IS NULL
                             AND fecha_inicio = @inicioOriginal
                             AND fecha_fin_original = @finOriginal
                             AND monto_dia = @montoOriginal
                             AND id_inquilino = @inquilino
                             AND id_inmueble = @inmueble";

                using var command = new MySqlCommand(sql, connection, transaction);
                command.Parameters.AddWithValue("@fechaFinalizacion", fechaFinalizacion.Date);
                command.Parameters.AddWithValue("@idUsuarioFinalizacion", pagoMulta.IdUsuarioCreacion);
                command.Parameters.AddWithValue("@idReserva", reserva.IdReserva);
                command.Parameters.AddWithValue("@inicioOriginal", reserva.FechaInicio.Date);
                command.Parameters.AddWithValue("@finOriginal", reserva.FechaFinOriginal.Date);
                command.Parameters.AddWithValue("@montoOriginal", reserva.MontoDia);
                command.Parameters.AddWithValue("@inquilino", reserva.IdInquilino);
                command.Parameters.AddWithValue("@inmueble", reserva.IdInmueble);
                int filas = command.ExecuteNonQuery();
                if (filas == 0)
                {
                    transaction.Rollback();
                    return 0;
                }
                pagoMulta.IdReserva = reserva.IdReserva;
                RepositorioPago.Insertar(pagoMulta, connection, transaction);
                transaction.Commit();
                return filas;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        private static Inmueble BloquearInmueble(int id, MySqlConnection connection, MySqlTransaction transaction)
        {
            // FOR UPDATE hace esperar otra reserva del mismo inmueble hasta terminar esta transacción.
            using var command = new MySqlCommand(
                "SELECT disponible, porcentaje_reserva FROM Inmueble WHERE id_inmueble = @id FOR UPDATE",
                connection, transaction);
            command.Parameters.AddWithValue("@id", id);
            using var reader = command.ExecuteReader();
            if (!reader.Read())
                throw new InvalidOperationException("El inmueble seleccionado ya no existe");
            return new Inmueble
            {
                IdInmueble = id,
                Disponible = Convert.ToBoolean(reader["disponible"]),
                PorcentajeReserva = Convert.ToDecimal(reader["porcentaje_reserva"])
            };
        }

        private static void ValidarDatos(Reserva reserva)
        {
            if (reserva.FechaInicio.Year < 1000 || reserva.FechaFinOriginal.Date <= reserva.FechaInicio.Date ||
                reserva.MontoDia <= 0 || reserva.MontoDia > 9999999999.99m || reserva.MontoDia != Math.Round(reserva.MontoDia, 2))
                throw new InvalidOperationException("Revisá las fechas y el monto diario de la reserva");
        }

        private static void ValidarOrigen(Reserva reserva, MySqlConnection connection, MySqlTransaction transaction)
        {
            if (!reserva.IdReservaOrigen.HasValue)
                return;
            using var command = new MySqlCommand(@"SELECT id_inquilino, id_inmueble,
                COALESCE(fecha_finalizacion_anticipada, fecha_fin_original) AS fin
                FROM Reserva WHERE id_reserva = @id FOR UPDATE", connection, transaction);
            command.Parameters.AddWithValue("@id", reserva.IdReservaOrigen.Value);
            using var reader = command.ExecuteReader();
            if (!reader.Read() || Convert.ToInt32(reader["id_inquilino"]) != reserva.IdInquilino ||
                Convert.ToInt32(reader["id_inmueble"]) != reserva.IdInmueble ||
                Convert.ToDateTime(reader["fin"]) > reserva.FechaInicio.Date)
                throw new InvalidOperationException("La reserva original cambió. Volvé a iniciar la renovación desde su detalle.");
        }

        public IList<Reserva> ObtenerLista(
            int pagina,
            int tamanoPagina,
            string? buscar,
            out int totalRegistros)
        {
            var lista = new List<Reserva>();
            string termino = buscar?.Trim() ?? "";
            int offset = (pagina - 1) * tamanoPagina;

            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            string tablas = @"FROM Reserva r
                              INNER JOIN Inquilino q
                                  ON r.id_inquilino = q.id_inquilino
                              INNER JOIN Inmueble i
                                  ON r.id_inmueble = i.id_inmueble";
            string filtro = @"WHERE @buscar = ''
                              OR CAST(r.id_reserva AS CHAR) LIKE @patron
                              OR q.dni LIKE @patron
                              OR q.nombre LIKE @patron
                              OR q.apellido LIKE @patron
                              OR i.direccion_inmueble LIKE @patron";

            using (var count = new MySqlCommand(
                $"SELECT COUNT(*) {tablas} {filtro}", connection))
            {
                count.Parameters.AddWithValue("@buscar", termino);
                count.Parameters.AddWithValue("@patron", $"%{termino}%");
                totalRegistros = Convert.ToInt32(count.ExecuteScalar());
            }

            string sql = $@"SELECT r.id_reserva, r.id_inquilino, r.id_inmueble,
                                   r.fecha_inicio, r.fecha_fin_original, r.monto_dia,
                                   r.fecha_finalizacion_anticipada,
                                   r.id_usuario_creacion, r.id_usuario_finalizacion,
                                   r.id_reserva_origen
                            {tablas}
                            {filtro}
                            ORDER BY r.fecha_inicio DESC, r.id_reserva DESC
                            LIMIT @tamano OFFSET @offset";
            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@buscar", termino);
            command.Parameters.AddWithValue("@patron", $"%{termino}%");
            command.Parameters.AddWithValue("@tamano", tamanoPagina);
            command.Parameters.AddWithValue("@offset", offset);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(Mapear(reader));
            }

            return lista;
        }

        public IList<Reserva> Buscar(string termino, int cantidad)
        {
            var lista = new List<Reserva>();
            using var connection = new MySqlConnection(connectionString);
            string sql = @"SELECT r.id_reserva, r.id_inquilino, r.id_inmueble,
                                  r.fecha_inicio, r.fecha_fin_original, r.monto_dia,
                                  r.fecha_finalizacion_anticipada,
                                  r.id_usuario_creacion, r.id_usuario_finalizacion,
                                  r.id_reserva_origen
                           FROM Reserva r
                           INNER JOIN Inquilino q
                               ON r.id_inquilino = q.id_inquilino
                           INNER JOIN Inmueble i
                               ON r.id_inmueble = i.id_inmueble
                           WHERE CAST(r.id_reserva AS CHAR) LIKE @patron
                              OR q.dni LIKE @patron
                              OR q.nombre LIKE @patron
                              OR q.apellido LIKE @patron
                              OR i.direccion_inmueble LIKE @patron
                           ORDER BY r.fecha_inicio DESC, r.id_reserva DESC
                           LIMIT @cantidad";
            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@patron", $"%{termino.Trim()}%");
            command.Parameters.AddWithValue("@cantidad", cantidad);
            connection.Open();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(Mapear(reader));
            }

            return lista;
        }

        private static Reserva Mapear(MySqlDataReader reader)
        {
            return new Reserva
            {
                IdReserva = Convert.ToInt32(reader["id_reserva"]),
                IdInquilino = Convert.ToInt32(reader["id_inquilino"]),
                IdInmueble = Convert.ToInt32(reader["id_inmueble"]),
                FechaInicio = Convert.ToDateTime(reader["fecha_inicio"]),
                FechaFinOriginal = Convert.ToDateTime(reader["fecha_fin_original"]),
                MontoDia = Convert.ToDecimal(reader["monto_dia"]),
                FechaFinalizacionAnticipada = reader["fecha_finalizacion_anticipada"] == DBNull.Value
                    ? null
                    : Convert.ToDateTime(reader["fecha_finalizacion_anticipada"]),
                IdUsuarioCreacion = reader["id_usuario_creacion"] == DBNull.Value
                    ? null
                    : Convert.ToInt32(reader["id_usuario_creacion"]),
                IdUsuarioFinalizacion = reader["id_usuario_finalizacion"] == DBNull.Value
                    ? null
                    : Convert.ToInt32(reader["id_usuario_finalizacion"]),
                IdReservaOrigen = reader["id_reserva_origen"] == DBNull.Value
                    ? null
                    : Convert.ToInt32(reader["id_reserva_origen"])
            };
        }
    }
}
