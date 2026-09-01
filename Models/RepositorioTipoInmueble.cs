using MySqlConnector;

namespace Inmobiliaria.Models
{
    public class RepositorioTipoInmueble : RepositorioBase, IRepositorioTipoInmueble
    {
        public RepositorioTipoInmueble(IConfiguration configuration)
            : base(configuration)
        {
        }

        public int Alta(TipoInmueble tipo)
        {
            int res = -1;

            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"INSERT INTO TipoInmueble
                    (nombre_tipo)
                    VALUES
                    (@nombreTipo);

                    SELECT LAST_INSERT_ID();";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@nombreTipo", tipo.NombreTipo);

                    connection.Open();

                    res = Convert.ToInt32(command.ExecuteScalar());

                    tipo.IdTipoInmueble = res;
                }
            }

            return res;
        }

        public int Baja(int id)
        {
            int res = -1;

            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"DELETE FROM TipoInmueble
                               WHERE id_tipo_inmueble = @id";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);

                    connection.Open();

                    res = command.ExecuteNonQuery();
                }
            }

            return res;
        }

        public int Modificacion(TipoInmueble tipo)
        {
            int res = -1;

            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"UPDATE TipoInmueble
                               SET nombre_tipo = @nombreTipo
                               WHERE id_tipo_inmueble = @id";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@nombreTipo", tipo.NombreTipo);
                    command.Parameters.AddWithValue("@id", tipo.IdTipoInmueble);

                    connection.Open();

                    res = command.ExecuteNonQuery();
                }
            }

            return res;
        }

        public IList<TipoInmueble> ObtenerLista()
        {
            IList<TipoInmueble> lista = new List<TipoInmueble>();

            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"SELECT
                                id_tipo_inmueble,
                                nombre_tipo
                               FROM TipoInmueble";

                using (var command = new MySqlCommand(sql, connection))
                {
                    connection.Open();

                    var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        TipoInmueble tipo = new TipoInmueble
                        {
                            IdTipoInmueble = Convert.ToInt32(reader["id_tipo_inmueble"]),
                            NombreTipo = reader["nombre_tipo"].ToString() ?? ""
                        };

                        lista.Add(tipo);
                    }
                }
            }

            return lista;
        }

        public TipoInmueble? ObtenerPorId(int id)
        {
            TipoInmueble? tipo = null;

            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"SELECT
                                id_tipo_inmueble,
                                nombre_tipo
                               FROM TipoInmueble
                               WHERE id_tipo_inmueble = @id";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);

                    connection.Open();

                    var reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        tipo = new TipoInmueble
                        {
                            IdTipoInmueble = Convert.ToInt32(reader["id_tipo_inmueble"]),
                            NombreTipo = reader["nombre_tipo"].ToString() ?? ""
                        };
                    }
                }
            }

            return tipo;
        }
    }
}