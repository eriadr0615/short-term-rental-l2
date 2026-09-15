using MySqlConnector;

namespace Inmobiliaria.Models
{
    public class RepositorioImagenInmueble : RepositorioBase, IRepositorioImagenInmueble
    {
        public RepositorioImagenInmueble(IConfiguration configuration)
            : base(configuration)
        {
        }

        public int Alta(ImagenInmueble imagen)
        {
            int res = -1;

            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"INSERT INTO Imagen_Inmueble
                               (id_inmueble, url_img, es_principal)
                               VALUES
                               (@idInmueble, @urlImg, @esPrincipal);

                               SELECT LAST_INSERT_ID();";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@idInmueble", imagen.IdInmueble);
                    command.Parameters.AddWithValue("@urlImg", imagen.UrlImg);
                    command.Parameters.AddWithValue("@esPrincipal", imagen.EsPrincipal);

                    connection.Open();

                    res = Convert.ToInt32(command.ExecuteScalar());

                    imagen.IdImagen = res;
                }
            }

            return res;
        }

        public int Baja(int id)
        {
            int res = -1;

            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"DELETE FROM Imagen_Inmueble
                               WHERE id_imagen = @id";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);

                    connection.Open();

                    res = command.ExecuteNonQuery();
                }
            }

            return res;
        }

        public int Modificacion(ImagenInmueble imagen)
        {
            int res = -1;

            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"UPDATE Imagen_Inmueble
                               SET id_inmueble = @idInmueble,
                                   url_img = @urlImg,
                                   es_principal = @esPrincipal
                               WHERE id_imagen = @id";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@idInmueble", imagen.IdInmueble);
                    command.Parameters.AddWithValue("@urlImg", imagen.UrlImg);
                    command.Parameters.AddWithValue("@esPrincipal", imagen.EsPrincipal);
                    command.Parameters.AddWithValue("@id", imagen.IdImagen);

                    connection.Open();

                    res = command.ExecuteNonQuery();
                }
            }

            return res;
        }

        public IList<ImagenInmueble> ObtenerLista()
        {
            IList<ImagenInmueble> lista = new List<ImagenInmueble>();

            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"SELECT
                               id_imagen,
                               id_inmueble,
                               url_img,
                               es_principal
                               FROM Imagen_Inmueble";

                using (var command = new MySqlCommand(sql, connection))
                {
                    connection.Open();

                    var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        ImagenInmueble imagen = new ImagenInmueble
                        {
                            IdImagen = Convert.ToInt32(reader["id_imagen"]),
                            IdInmueble = Convert.ToInt32(reader["id_inmueble"]),
                            UrlImg = reader["url_img"].ToString() ?? "",
                            EsPrincipal = Convert.ToBoolean(reader["es_principal"])
                        };

                        lista.Add(imagen);
                    }
                }
            }

            return lista;
        }

        public ImagenInmueble? ObtenerPorId(int id)
        {
            ImagenInmueble? imagen = null;

            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"SELECT
                               id_imagen,
                               id_inmueble,
                               url_img,
                               es_principal
                               FROM Imagen_Inmueble
                               WHERE id_imagen = @id";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);

                    connection.Open();

                    var reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        imagen = new ImagenInmueble
                        {
                            IdImagen = Convert.ToInt32(reader["id_imagen"]),
                            IdInmueble = Convert.ToInt32(reader["id_inmueble"]),
                            UrlImg = reader["url_img"].ToString() ?? "",
                            EsPrincipal = Convert.ToBoolean(reader["es_principal"])
                        };
                    }
                }
            }

            return imagen;
        }

        public IList<ImagenInmueble> ObtenerPorInmueble(int idInmueble)
        {
            IList<ImagenInmueble> lista = new List<ImagenInmueble>();

            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"SELECT
                               id_imagen,
                               id_inmueble,
                               url_img,
                               es_principal
                               FROM Imagen_Inmueble
                               WHERE id_inmueble = @idInmueble";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@idInmueble", idInmueble);

                    connection.Open();

                    var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        ImagenInmueble imagen = new ImagenInmueble
                        {
                            IdImagen = Convert.ToInt32(reader["id_imagen"]),
                            IdInmueble = Convert.ToInt32(reader["id_inmueble"]),
                            UrlImg = reader["url_img"].ToString() ?? "",
                            EsPrincipal = Convert.ToBoolean(reader["es_principal"])
                        };

                        lista.Add(imagen);
                    }
                }
            }

            return lista;
        }

        public int QuitarPrincipal(int idInmueble)
        {
            int res = -1;

            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"UPDATE Imagen_Inmueble
                               SET es_principal = false
                               WHERE id_inmueble = @idInmueble";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@idInmueble", idInmueble);

                    connection.Open();

                    res = command.ExecuteNonQuery();
                }
            }

            return res;
        }
    }
}