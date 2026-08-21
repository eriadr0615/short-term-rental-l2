using MySqlConnector;
using System.Data;

namespace Inmobiliaria.Models
{
    public class RepositorioPropietario : RepositorioBase, IRepositorioPropietario
    {
        public RepositorioPropietario(IConfiguration configuration)
            : base(configuration)
        {
        }

        public int Alta(Propietario p)
        {
            int res = -1;

            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"INSERT INTO Propietario
                    (dni, nombre, apellido, telefono, correo, direccion)
                    VALUES
                    (@dni, @nombre, @apellido, @telefono, @correo, @direccion);

                    SELECT LAST_INSERT_ID();";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@dni", p.Dni);
                    command.Parameters.AddWithValue("@nombre", p.Nombre);
                    command.Parameters.AddWithValue("@apellido", p.Apellido);
                    command.Parameters.AddWithValue("@telefono", p.Telefono);
                    command.Parameters.AddWithValue("@correo", p.Correo);
                    command.Parameters.AddWithValue("@direccion", p.Direccion);

                    connection.Open();

                    res = Convert.ToInt32(command.ExecuteScalar());

                    p.IdPropietario = res;
                }
            }

            return res;
        }

        public int Baja(int id)
        {
            int res = -1;

            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"DELETE FROM Propietario
                               WHERE id_propietario = @id";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);

                    connection.Open();

                    res = command.ExecuteNonQuery();
                }
            }

            return res;
        }

        public int Modificacion(Propietario p)
        {
            int res = -1;

            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"UPDATE Propietario
                               SET dni = @dni,
                                   nombre = @nombre,
                                   apellido = @apellido,
                                   telefono = @telefono,
                                   correo = @correo,
                                   direccion = @direccion
                               WHERE id_propietario = @id";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@dni", p.Dni);
                    command.Parameters.AddWithValue("@nombre", p.Nombre);
                    command.Parameters.AddWithValue("@apellido", p.Apellido);
                    command.Parameters.AddWithValue("@telefono", p.Telefono);
                    command.Parameters.AddWithValue("@correo", p.Correo);
                    command.Parameters.AddWithValue("@direccion", p.Direccion);
                    command.Parameters.AddWithValue("@id", p.IdPropietario);

                    connection.Open();

                    res = command.ExecuteNonQuery();
                }
            }

            return res;
        }

        public IList<Propietario> ObtenerLista()
        {
            IList<Propietario> lista = new List<Propietario>();

            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"SELECT
                                id_propietario,
                                dni,
                                nombre,
                                apellido,
                                telefono,
                                correo,
                                direccion
                               FROM Propietario";

                using (var command = new MySqlCommand(sql, connection))
                {
                    connection.Open();

                    var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        Propietario p = new Propietario
                        {
                            IdPropietario = reader.GetInt32("id_propietario"),
                            Dni = reader.GetString("dni"),
                            Nombre = reader.GetString("nombre"),
                            Apellido = reader.GetString("apellido"),
                            Telefono = reader.IsDBNull("telefono") ? "" : reader.GetString("telefono"),
                            Correo = reader.IsDBNull("correo") ? "" : reader.GetString("correo"),
                            Direccion = reader.IsDBNull("direccion") ? "" : reader.GetString("direccion")
                        };

                        lista.Add(p);
                    }
                }
            }

            return lista;
        }

        public Propietario? ObtenerPorId(int id)
        {
            Propietario? p = null;

            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"SELECT
                                id_propietario,
                                dni,
                                nombre,
                                apellido,
                                telefono,
                                correo,
                                direccion
                               FROM Propietario
                               WHERE id_propietario = @id";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);

                    connection.Open();

                    var reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        p = new Propietario
                        {
                            IdPropietario = reader.GetInt32("id_propietario"),
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

            return p;
        }
    }
}