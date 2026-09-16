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
                            IdPropietario = Convert.ToInt32(reader["id_propietario"]),
                            Dni = reader["dni"].ToString() ?? "",
                            Nombre = reader["nombre"].ToString() ?? "",
                            Apellido = reader["apellido"].ToString() ?? "",
                            Telefono = reader["telefono"].ToString() ?? "",
                            Correo = reader["correo"].ToString() ?? "",
                            Direccion = reader["direccion"].ToString() ?? ""
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
                            IdPropietario = Convert.ToInt32(reader["id_propietario"]),
                            Dni = reader["dni"].ToString() ?? "",
                            Nombre = reader["nombre"].ToString() ?? "",
                            Apellido = reader["apellido"].ToString() ?? "",
                            Telefono = reader["telefono"].ToString() ?? "",
                            Correo = reader["correo"].ToString() ?? "",
                            Direccion = reader["direccion"].ToString() ?? ""
                        };
                    }
                }
            }

            return p;
        }

        public IList<Propietario> ObtenerLista(
            int pagina,
            int tamanoPagina,
            string? buscar,
            out int totalRegistros)
        {
            var lista = new List<Propietario>();
            string termino = buscar?.Trim() ?? "";
            int offset = (pagina - 1) * tamanoPagina;

            using var connection = new MySqlConnection(connectionString);
            connection.Open();

            string filtro = @"WHERE @buscar = ''
                              OR dni LIKE @patron
                              OR nombre LIKE @patron
                              OR apellido LIKE @patron
                              OR correo LIKE @patron";

            using (var count = new MySqlCommand(
                $"SELECT COUNT(*) FROM Propietario {filtro}", connection))
            {
                count.Parameters.AddWithValue("@buscar", termino);
                count.Parameters.AddWithValue("@patron", $"%{termino}%");
                totalRegistros = Convert.ToInt32(count.ExecuteScalar());
            }

            string sql = $@"SELECT id_propietario, dni, nombre, apellido,
                                   telefono, correo, direccion
                            FROM Propietario
                            {filtro}
                            ORDER BY apellido, nombre, id_propietario
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

        public IList<Propietario> Buscar(string termino, int cantidad)
        {
            var lista = new List<Propietario>();
            using var connection = new MySqlConnection(connectionString);
            string sql = @"SELECT id_propietario, dni, nombre, apellido,
                                  telefono, correo, direccion
                           FROM Propietario
                           WHERE dni LIKE @patron
                              OR nombre LIKE @patron
                              OR apellido LIKE @patron
                              OR CONCAT(nombre, ' ', apellido) LIKE @patron
                           ORDER BY apellido, nombre
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

        private static Propietario Mapear(MySqlDataReader reader)
        {
            return new Propietario
            {
                IdPropietario = Convert.ToInt32(reader["id_propietario"]),
                Dni = reader["dni"].ToString() ?? "",
                Nombre = reader["nombre"].ToString() ?? "",
                Apellido = reader["apellido"].ToString() ?? "",
                Telefono = reader["telefono"].ToString() ?? "",
                Correo = reader["correo"].ToString() ?? "",
                Direccion = reader["direccion"].ToString() ?? ""
            };
        }
    }
}
