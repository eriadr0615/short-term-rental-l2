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
            int res = -1;

            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"INSERT INTO Reserva
                    (id_inquilino,
                     id_inmueble,
                     fecha_inicio,
                     fecha_fin_original,
                     monto_dia,
                     fecha_finalizacion_anticipada,
                     id_usuario_creacion,
                     id_usuario_finalizacion,
                     id_reserva_origen)
                    VALUES
                    (@idInquilino,
                     @idInmueble,
                     @fechaInicio,
                     @fechaFinOriginal,
                     @montoDia,
                     @fechaFinalizacionAnticipada,
                     @idUsuarioCreacion,
                     @idUsuarioFinalizacion,
                     @idReservaOrigen);

                    SELECT LAST_INSERT_ID();";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@idInquilino", reserva.IdInquilino);
                    command.Parameters.AddWithValue("@idInmueble", reserva.IdInmueble);
                    command.Parameters.AddWithValue("@fechaInicio", reserva.FechaInicio);
                    command.Parameters.AddWithValue("@fechaFinOriginal", reserva.FechaFinOriginal);
                    command.Parameters.AddWithValue("@montoDia", reserva.MontoDia);
                    command.Parameters.AddWithValue("@fechaFinalizacionAnticipada", (object?)reserva.FechaFinalizacionAnticipada ?? DBNull.Value);
                    command.Parameters.AddWithValue("@idUsuarioCreacion", (object?)reserva.IdUsuarioCreacion ?? DBNull.Value);
                    command.Parameters.AddWithValue("@idUsuarioFinalizacion", (object?)reserva.IdUsuarioFinalizacion ?? DBNull.Value);
                    command.Parameters.AddWithValue("@idReservaOrigen", (object?)reserva.IdReservaOrigen ?? DBNull.Value);

                    connection.Open();

                    res = Convert.ToInt32(command.ExecuteScalar());

                    reserva.IdReserva = res;
                }
            }

            return res;
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
            int res = -1;

            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"UPDATE Reserva
                               SET id_inquilino = @idInquilino,
                                   id_inmueble = @idInmueble,
                                   fecha_inicio = @fechaInicio,
                                   fecha_fin_original = @fechaFinOriginal,
                                   monto_dia = @montoDia,
                                   fecha_finalizacion_anticipada = @fechaFinalizacionAnticipada,
                                   id_usuario_creacion = @idUsuarioCreacion,
                                   id_usuario_finalizacion = @idUsuarioFinalizacion,
                                   id_reserva_origen = @idReservaOrigen
                               WHERE id_reserva = @id";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@idInquilino", reserva.IdInquilino);
                    command.Parameters.AddWithValue("@idInmueble", reserva.IdInmueble);
                    command.Parameters.AddWithValue("@fechaInicio", reserva.FechaInicio);
                    command.Parameters.AddWithValue("@fechaFinOriginal", reserva.FechaFinOriginal);
                    command.Parameters.AddWithValue("@montoDia", reserva.MontoDia);
                    command.Parameters.AddWithValue("@fechaFinalizacionAnticipada", (object?)reserva.FechaFinalizacionAnticipada ?? DBNull.Value);
                    command.Parameters.AddWithValue("@idUsuarioCreacion", (object?)reserva.IdUsuarioCreacion ?? DBNull.Value);
                    command.Parameters.AddWithValue("@idUsuarioFinalizacion", (object?)reserva.IdUsuarioFinalizacion ?? DBNull.Value);
                    command.Parameters.AddWithValue("@idReservaOrigen", (object?)reserva.IdReservaOrigen ?? DBNull.Value);
                    command.Parameters.AddWithValue("@id", reserva.IdReserva);

                    connection.Open();

                    res = command.ExecuteNonQuery();
                }
            }

            return res;
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
    }
}
