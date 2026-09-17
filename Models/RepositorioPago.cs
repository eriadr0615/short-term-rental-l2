using MySqlConnector;

namespace Inmobiliaria.Models
{
    public class RepositorioPago : RepositorioBase, IRepositorioPago
    {
        public RepositorioPago(IConfiguration configuration)
            : base(configuration)
        {
        }

        public int Alta(Pago pago)
        {
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            return Insertar(pago, connection, null);
        }

        // Permite compartir la conexión y la transacción con una reserva.
        internal static int Insertar(Pago pago, MySqlConnection connection, MySqlTransaction? transaction)
        {
            string sql = @"INSERT INTO Pago
                           (id_reserva, concepto, fecha_pago, monto, estado,
                            id_usuario_creacion, id_usuario_anulacion)
                           VALUES
                           (@idReserva, @concepto, @fechaPago, @monto, @estado,
                            @idUsuarioCreacion, NULL);
                           SELECT LAST_INSERT_ID();";

            using var command = new MySqlCommand(sql, connection, transaction);
            command.Parameters.AddWithValue("@idReserva", pago.IdReserva);
            command.Parameters.AddWithValue("@concepto", pago.Concepto);
            command.Parameters.AddWithValue("@fechaPago", pago.FechaPago);
            command.Parameters.AddWithValue("@monto", pago.Monto);
            command.Parameters.AddWithValue("@estado", Pago.EstadoActivo);
            command.Parameters.AddWithValue("@idUsuarioCreacion", pago.IdUsuarioCreacion);

            pago.IdPago = Convert.ToInt32(command.ExecuteScalar());
            return pago.IdPago;
        }

        public int Baja(int id)
        {
            using var connection = new MySqlConnection(connectionString);
            string sql = @"UPDATE Pago
                           SET estado = @estado
                           WHERE id_pago = @id
                             AND estado = @estadoActivo";

            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@estado", Pago.EstadoAnulado);
            command.Parameters.AddWithValue("@estadoActivo", Pago.EstadoActivo);
            command.Parameters.AddWithValue("@id", id);
            connection.Open();
            return command.ExecuteNonQuery();
        }

        public int Modificacion(Pago pago)
        {
            using var connection = new MySqlConnection(connectionString);
            string sql = @"UPDATE Pago
                           SET concepto = @concepto
                           WHERE id_pago = @id
                             AND estado = @estadoActivo";

            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@concepto", pago.Concepto);
            command.Parameters.AddWithValue("@id", pago.IdPago);
            command.Parameters.AddWithValue("@estadoActivo", Pago.EstadoActivo);
            connection.Open();
            return command.ExecuteNonQuery();
        }

        public IList<Pago> ObtenerLista()
        {
            var lista = new List<Pago>();
            using var connection = new MySqlConnection(connectionString);
            string sql = @"SELECT id_pago, id_reserva, concepto, fecha_pago, monto,
                                  estado, id_usuario_creacion, id_usuario_anulacion
                           FROM Pago
                           ORDER BY fecha_pago DESC, id_pago DESC";

            using var command = new MySqlCommand(sql, connection);
            connection.Open();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(Mapear(reader));
            }

            return lista;
        }

        public Pago? ObtenerPorId(int id)
        {
            using var connection = new MySqlConnection(connectionString);
            string sql = @"SELECT id_pago, id_reserva, concepto, fecha_pago, monto,
                                  estado, id_usuario_creacion, id_usuario_anulacion
                           FROM Pago
                           WHERE id_pago = @id";

            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@id", id);
            connection.Open();
            using var reader = command.ExecuteReader();
            return reader.Read() ? Mapear(reader) : null;
        }

        public IList<Pago> ObtenerPorReserva(int idReserva)
        {
            var lista = new List<Pago>();
            using var connection = new MySqlConnection(connectionString);
            string sql = @"SELECT id_pago, id_reserva, concepto, fecha_pago, monto,
                                  estado, id_usuario_creacion, id_usuario_anulacion
                           FROM Pago
                           WHERE id_reserva = @idReserva
                           ORDER BY fecha_pago DESC, id_pago DESC";

            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@idReserva", idReserva);
            connection.Open();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(Mapear(reader));
            }

            return lista;
        }

        public int Anular(int idPago, int idUsuarioAnulacion)
        {
            using var connection = new MySqlConnection(connectionString);
            string sql = @"UPDATE Pago
                           SET estado = @estadoAnulado,
                               id_usuario_anulacion = @idUsuarioAnulacion
                           WHERE id_pago = @idPago
                             AND estado = @estadoActivo";

            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@estadoAnulado", Pago.EstadoAnulado);
            command.Parameters.AddWithValue("@idUsuarioAnulacion", idUsuarioAnulacion);
            command.Parameters.AddWithValue("@idPago", idPago);
            command.Parameters.AddWithValue("@estadoActivo", Pago.EstadoActivo);
            connection.Open();
            return command.ExecuteNonQuery();
        }

        public IList<Pago> ObtenerLista(
            int pagina,
            int tamanoPagina,
            string? buscar,
            int? idReserva,
            out int totalRegistros)
        {
            var lista = new List<Pago>();
            string termino = buscar?.Trim() ?? "";
            int offset = (pagina - 1) * tamanoPagina;

            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            string tablas = @"FROM Pago p
                              INNER JOIN Reserva r ON p.id_reserva = r.id_reserva
                              INNER JOIN Inquilino q ON r.id_inquilino = q.id_inquilino
                              INNER JOIN Inmueble i ON r.id_inmueble = i.id_inmueble";
            string filtro = @"WHERE (@idReserva IS NULL OR p.id_reserva = @idReserva)
                              AND (
                                  @buscar = ''
                                  OR p.concepto LIKE @patron
                                  OR p.estado LIKE @patron
                                  OR CAST(p.id_reserva AS CHAR) LIKE @patron
                                  OR q.dni LIKE @patron
                                  OR q.nombre LIKE @patron
                                  OR q.apellido LIKE @patron
                                  OR i.direccion_inmueble LIKE @patron
                              )";

            using (var count = new MySqlCommand(
                $"SELECT COUNT(*) {tablas} {filtro}", connection))
            {
                AgregarParametrosFiltro(count, termino, idReserva);
                totalRegistros = Convert.ToInt32(count.ExecuteScalar());
            }

            string sql = $@"SELECT p.id_pago, p.id_reserva, p.concepto,
                                   p.fecha_pago, p.monto, p.estado,
                                   p.id_usuario_creacion, p.id_usuario_anulacion
                            {tablas}
                            {filtro}
                            ORDER BY p.fecha_pago DESC, p.id_pago DESC
                            LIMIT @tamano OFFSET @offset";
            using var command = new MySqlCommand(sql, connection);
            AgregarParametrosFiltro(command, termino, idReserva);
            command.Parameters.AddWithValue("@tamano", tamanoPagina);
            command.Parameters.AddWithValue("@offset", offset);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(Mapear(reader));
            }

            return lista;
        }

        private static void AgregarParametrosFiltro(
            MySqlCommand command,
            string termino,
            int? idReserva)
        {
            command.Parameters.AddWithValue(
                "@idReserva", (object?)idReserva ?? DBNull.Value);
            command.Parameters.AddWithValue("@buscar", termino);
            command.Parameters.AddWithValue("@patron", $"%{termino}%");
        }

        private static Pago Mapear(MySqlDataReader reader)
        {
            return new Pago
            {
                IdPago = Convert.ToInt32(reader["id_pago"]),
                IdReserva = Convert.ToInt32(reader["id_reserva"]),
                Concepto = reader["concepto"].ToString() ?? "",
                FechaPago = Convert.ToDateTime(reader["fecha_pago"]),
                Monto = Convert.ToDecimal(reader["monto"]),
                Estado = reader["estado"].ToString() ?? Pago.EstadoActivo,
                IdUsuarioCreacion = Convert.ToInt32(reader["id_usuario_creacion"]),
                IdUsuarioAnulacion = reader["id_usuario_anulacion"] == DBNull.Value
                    ? null
                    : Convert.ToInt32(reader["id_usuario_anulacion"])
            };
        }
    }
}
