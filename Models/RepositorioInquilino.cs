using MySqlConnector;

namespace Inmobiliaria.Models
{
    public class RepositorioInquilino : RepositorioBase, IRepositorioInquilino
    {
        public RepositorioInquilino(IConfiguration configuration)
            : base(configuration)
        {
        }

        public int Alta(Inquilino i)
        {
            int res = -1;

            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"INSERT INTO Inquilino
                    (dni, nombre, apellido, telefono, correo, direccion)
                    VALUES
                    (@dni, @nombre, @apellido, @telefono, @correo, @direccion);

                    SELECT LAST_INSERT_ID();";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@dni", i.Dni);
                    command.Parameters.AddWithValue("@nombre", i.Nombre);
                    command.Parameters.AddWithValue("@apellido", i.Apellido);
                    command.Parameters.AddWithValue("@telefono", i.Telefono);
                    command.Parameters.AddWithValue("@correo", i.Correo);
                    command.Parameters.AddWithValue("@direccion", i.Direccion);

                    connection.Open();

                    res = Convert.ToInt32(command.ExecuteScalar());

                    i.IdInquilino = res;
                }
            }

            return res;
        }

        public int Baja(int id)
        {
            int res = -1;

            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"DELETE FROM Inquilino
                               WHERE id_inquilino = @id";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);

                    connection.Open();

                    res = command.ExecuteNonQuery();
                }
            }

            return res;
        }

        public int Modificacion(Inquilino i)
        {
            int res = -1;

            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"UPDATE Inquilino
                               SET dni = @dni,
                                   nombre = @nombre,
                                   apellido = @apellido,
                                   telefono = @telefono,
                                   correo = @correo,
                                   direccion = @direccion
                               WHERE id_inquilino = @id";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@dni", i.Dni);
                    command.Parameters.AddWithValue("@nombre", i.Nombre);
                    command.Parameters.AddWithValue("@apellido", i.Apellido);
                    command.Parameters.AddWithValue("@telefono", i.Telefono);
                    command.Parameters.AddWithValue("@correo", i.Correo);
                    command.Parameters.AddWithValue("@direccion", i.Direccion);
                    command.Parameters.AddWithValue("@id", i.IdInquilino);

                    connection.Open();

                    res = command.ExecuteNonQuery();
                }
            }

            return res;
        }

        public IList<Inquilino> ObtenerLista()
        {
            IList<Inquilino> lista = new List<Inquilino>();

            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"SELECT
                                id_inquilino,
                                dni,
                                nombre,
                                apellido,
                                telefono,
                                correo,
                                direccion
                               FROM Inquilino";

                using (var command = new MySqlCommand(sql, connection))
                {
                    connection.Open();

                    var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        Inquilino i = new Inquilino
                        {
                            IdInquilino = reader.GetInt32("id_inquilino"),
                            Dni = reader.GetString("dni"),
                            Nombre = reader.GetString("nombre"),
                            Apellido = reader.GetString("apellido"),
                            Telefono = reader.IsDBNull("telefono") ? "" : reader.GetString("telefono"),
                            Correo = reader.IsDBNull("correo") ? "" : reader.GetString("correo"),
                            Direccion = reader.IsDBNull("direccion") ? "" : reader.GetString("direccion")
                        };

                        lista.Add(i);
                    }
                }
            }

            return lista;
        }

        public Inquilino? ObtenerPorId(int id)
        {
            Inquilino? i = null;

            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"SELECT
                                id_inquilino,
                                dni,
                                nombre,
                                apellido,
                                telefono,
                                correo,
                                direccion
                               FROM Inquilino
                               WHERE id_inquilino = @id";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);

                    connection.Open();

                    var reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        i = new Inquilino
                        {
                            IdInquilino = reader.GetInt32("id_inquilino"),
                            Dni = reader.GetString("dni"),
                            Nombre = reader.GetString("nombre"),
                            Apellido = reader.GetString("apellido"),
                            Telefono = reader.IsDBNull("telefono") ? "" : reader.GetString("telefono"),
                            Correo = reader.IsDBNull("correo") ? "" : reader.GetString("correo"),
                            Direccion = reader.IsDBNull("direccion") ? "" : reader.GetString("direccion")
                        };
                    }
                }
            }

            return i;
        }
    }
}