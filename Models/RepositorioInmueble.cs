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
    }
}