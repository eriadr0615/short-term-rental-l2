using MySqlConnector;

namespace Inmobiliaria.Models
{
    public class RepositorioInmueble : RepositorioBase, IRepositorioInmueble
    {
        public RepositorioInmueble(IConfiguration configuration)
            : base(configuration)
        {
        }

        public int Alta(Inmueble inmueble)
        {
            int res = -1;
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"INSERT INTO Inmueble
                    (id_propietario,
                     direccion_inmueble,
                     id_tipo_inmueble,
                     coordenadas_inmuebles,
                     precio_diario,
                     porcentaje_reserva,
                     disponible,
                     capacidad_maxima)
                    VALUES
                    (@idPropietario,
                     @direccion,
                     @idTipoInmueble,
                     @coordenadas,
                     @precioDiario,
                     @porcentajeReserva,
                     @disponible,
                     @capacidadMaxima);

                    SELECT LAST_INSERT_ID();";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@idPropietario", inmueble.IdPropietario);
                    command.Parameters.AddWithValue("@direccion", inmueble.DireccionInmueble);
                    command.Parameters.AddWithValue("@idTipoInmueble", inmueble.IdTipoInmueble);
                    command.Parameters.AddWithValue("@coordenadas", inmueble.CoordenadasInmuebles);
                    command.Parameters.AddWithValue("@precioDiario", inmueble.PrecioDiario);
                    command.Parameters.AddWithValue("@porcentajeReserva", inmueble.PorcentajeReserva);
                    command.Parameters.AddWithValue("@disponible", inmueble.Disponible);
                    command.Parameters.AddWithValue("@capacidadMaxima", inmueble.CapacidadMaxima);

                    connection.Open();

                    res = Convert.ToInt32(command.ExecuteScalar());
                    inmueble.IdInmueble = res;
                }
            }

            return res;
        }

        public int Baja(int id)
        {
            int res = -1;
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"DELETE FROM Inmueble
                               WHERE id_inmueble = @id";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);

                    connection.Open();

                    res = command.ExecuteNonQuery();
                }
            }
            return res;
        }

        public int Modificacion(Inmueble inmueble)
        {
            int res = -1;
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"UPDATE Inmueble
                               SET id_propietario = @idPropietario,
                                   direccion_inmueble = @direccion,
                                   id_tipo_inmueble = @idTipoInmueble,
                                   coordenadas_inmuebles = @coordenadas,
                                   precio_diario = @precioDiario,
                                   porcentaje_reserva = @porcentajeReserva,
                                   disponible = @disponible,
                                   capacidad_maxima = @capacidadMaxima
                               WHERE id_inmueble = @id";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@idPropietario", inmueble.IdPropietario);
                    command.Parameters.AddWithValue("@direccion", inmueble.DireccionInmueble);
                    command.Parameters.AddWithValue("@idTipoInmueble", inmueble.IdTipoInmueble);
                    command.Parameters.AddWithValue("@coordenadas", inmueble.CoordenadasInmuebles);
                    command.Parameters.AddWithValue("@precioDiario", inmueble.PrecioDiario);
                    command.Parameters.AddWithValue("@porcentajeReserva", inmueble.PorcentajeReserva);
                    command.Parameters.AddWithValue("@disponible", inmueble.Disponible);
                    command.Parameters.AddWithValue("@capacidadMaxima", inmueble.CapacidadMaxima);
                    command.Parameters.AddWithValue("@id", inmueble.IdInmueble);

                    connection.Open();

                    res = command.ExecuteNonQuery();
                }
            }
            return res;
        }

        public IList<Inmueble> ObtenerLista()
        {
            IList<Inmueble> lista = new List<Inmueble>();
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"SELECT
                                id_inmueble,
                                id_propietario,
                                direccion_inmueble,
                                id_tipo_inmueble,
                                coordenadas_inmuebles,
                                precio_diario,
                                porcentaje_reserva,
                                disponible,
                                capacidad_maxima
                               FROM Inmueble";

                using (var command = new MySqlCommand(sql, connection))
                {
                    connection.Open();
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        Inmueble inmueble = new Inmueble
                        {
                            IdInmueble = Convert.ToInt32(reader["id_inmueble"]),
                            IdPropietario = Convert.ToInt32(reader["id_propietario"]),
                            DireccionInmueble = reader["direccion_inmueble"].ToString() ?? "",
                            IdTipoInmueble = Convert.ToInt32(reader["id_tipo_inmueble"]),
                            CoordenadasInmuebles = reader["coordenadas_inmuebles"].ToString() ?? "",
                            PrecioDiario = Convert.ToDecimal(reader["precio_diario"]),
                            PorcentajeReserva = Convert.ToDecimal(reader["porcentaje_reserva"]),
                            Disponible = Convert.ToBoolean(reader["disponible"]),
                            CapacidadMaxima = Convert.ToInt32(reader["capacidad_maxima"])
                        };
                        lista.Add(inmueble);
                    }
                }
            }
            return lista;
        }

        public Inmueble? ObtenerPorId(int id)
        {
            Inmueble? inmueble = null;

            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"SELECT
                                id_inmueble,
                                id_propietario,
                                direccion_inmueble,
                                id_tipo_inmueble,
                                coordenadas_inmuebles,
                                precio_diario,
                                porcentaje_reserva,
                                disponible,
                                capacidad_maxima
                               FROM Inmueble
                               WHERE id_inmueble = @id";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);

                    connection.Open();

                    var reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        inmueble = new Inmueble
                        {
                            IdInmueble = Convert.ToInt32(reader["id_inmueble"]),
                            IdPropietario = Convert.ToInt32(reader["id_propietario"]),
                            DireccionInmueble = reader["direccion_inmueble"].ToString() ?? "",
                            IdTipoInmueble = Convert.ToInt32(reader["id_tipo_inmueble"]),
                            CoordenadasInmuebles = reader["coordenadas_inmuebles"].ToString() ?? "",
                            PrecioDiario = Convert.ToDecimal(reader["precio_diario"]),
                            PorcentajeReserva = Convert.ToDecimal(reader["porcentaje_reserva"]),
                            Disponible = Convert.ToBoolean(reader["disponible"]),
                            CapacidadMaxima = Convert.ToInt32(reader["capacidad_maxima"])
                        };
                    }
                }
            }

            return inmueble;
        }

        public IList<Inmueble> ObtenerLista(
            int pagina,
            int tamanoPagina,
            string? buscar,
            out int totalRegistros)
        {
            var lista = new List<Inmueble>();
            string termino = buscar?.Trim() ?? "";
            int offset = (pagina - 1) * tamanoPagina;

            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            string filtro = @"WHERE @buscar = ''
                              OR i.direccion_inmueble LIKE @patron
                              OR t.nombre_tipo LIKE @patron";
            using (var count = new MySqlCommand(
                $@"SELECT COUNT(*)
                   FROM Inmueble i
                   INNER JOIN TipoInmueble t
                       ON i.id_tipo_inmueble = t.id_tipo_inmueble
                   {filtro}", connection))
            {
                count.Parameters.AddWithValue("@buscar", termino);
                count.Parameters.AddWithValue("@patron", $"%{termino}%");
                totalRegistros = Convert.ToInt32(count.ExecuteScalar());
            }

            string sql = $@"SELECT i.id_inmueble, i.id_propietario,
                                   i.direccion_inmueble, i.id_tipo_inmueble,
                                   i.coordenadas_inmuebles, i.precio_diario,
                                   i.porcentaje_reserva, i.disponible,
                                   i.capacidad_maxima
                            FROM Inmueble i
                            INNER JOIN TipoInmueble t
                                ON i.id_tipo_inmueble = t.id_tipo_inmueble
                            {filtro}
                            ORDER BY i.direccion_inmueble, i.id_inmueble
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

        public IList<Inmueble> BuscarDisponibles(
            string termino,
            DateTime? fechaInicio,
            DateTime? fechaFin,
            int? idReservaExcluir,
            int cantidad)
        {
            var lista = new List<Inmueble>();
            using var connection = new MySqlConnection(connectionString);
            string sql = @"SELECT i.id_inmueble, i.id_propietario,
                                  i.direccion_inmueble, i.id_tipo_inmueble,
                                  i.coordenadas_inmuebles, i.precio_diario,
                                  i.porcentaje_reserva, i.disponible,
                                  i.capacidad_maxima
                           FROM Inmueble i
                           WHERE i.disponible = TRUE
                             AND i.direccion_inmueble LIKE @patron
                             AND (
                                 @fechaInicio IS NULL
                                 OR @fechaFin IS NULL
                                 OR NOT EXISTS (
                                     SELECT 1
                                     FROM Reserva r
                                     WHERE r.id_inmueble = i.id_inmueble
                                       AND r.fecha_inicio < @fechaFin
                                       AND COALESCE(r.fecha_finalizacion_anticipada,
                                                    r.fecha_fin_original) > @fechaInicio
                                       AND (@idReservaExcluir IS NULL
                                            OR r.id_reserva <> @idReservaExcluir)
                                 )
                             )
                           ORDER BY i.direccion_inmueble
                           LIMIT @cantidad";
            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@patron", $"%{termino.Trim()}%");
            command.Parameters.AddWithValue(
                "@fechaInicio", (object?)fechaInicio ?? DBNull.Value);
            command.Parameters.AddWithValue(
                "@fechaFin", (object?)fechaFin ?? DBNull.Value);
            command.Parameters.AddWithValue(
                "@idReservaExcluir", (object?)idReservaExcluir ?? DBNull.Value);
            command.Parameters.AddWithValue("@cantidad", cantidad);
            connection.Open();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(Mapear(reader));
            }

            return lista;
        }

        private static Inmueble Mapear(MySqlDataReader reader)
        {
            return new Inmueble
            {
                IdInmueble = Convert.ToInt32(reader["id_inmueble"]),
                IdPropietario = Convert.ToInt32(reader["id_propietario"]),
                DireccionInmueble = reader["direccion_inmueble"].ToString() ?? "",
                IdTipoInmueble = Convert.ToInt32(reader["id_tipo_inmueble"]),
                CoordenadasInmuebles = reader["coordenadas_inmuebles"].ToString() ?? "",
                PrecioDiario = Convert.ToDecimal(reader["precio_diario"]),
                PorcentajeReserva = Convert.ToDecimal(reader["porcentaje_reserva"]),
                Disponible = Convert.ToBoolean(reader["disponible"]),
                CapacidadMaxima = Convert.ToInt32(reader["capacidad_maxima"])
            };
        }
    }
}
