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
                            IdInquilino = Convert.ToInt32(reader["id_inquilino"]),
                            Dni = reader["dni"].ToString() ?? "",
                            Nombre = reader["nombre"].ToString() ?? "",
                            Apellido = reader["apellido"].ToString() ?? "",
                            Telefono = reader["telefono"].ToString() ?? "",
                            Correo = reader["correo"].ToString() ?? "",
                            Direccion = reader["direccion"].ToString() ?? ""
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
                            IdInquilino = Convert.ToInt32(reader["id_inquilino"]),
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

            return i;
        }

        public IList<Inquilino> ObtenerLista(
            int pagina,
            int tamanoPagina,
            string? buscar,
            out int totalRegistros)
        {
            var lista = new List<Inquilino>();
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
                $"SELECT COUNT(*) FROM Inquilino {filtro}", connection))
            {
                count.Parameters.AddWithValue("@buscar", termino);
                count.Parameters.AddWithValue("@patron", $"%{termino}%");
                totalRegistros = Convert.ToInt32(count.ExecuteScalar());
            }

            string sql = $@"SELECT id_inquilino, dni, nombre, apellido,
                                   telefono, correo, direccion
                            FROM Inquilino
                            {filtro}
                            ORDER BY apellido, nombre, id_inquilino
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

        public IList<Inquilino> Buscar(string termino, int cantidad)
        {
            var lista = new List<Inquilino>();
            using var connection = new MySqlConnection(connectionString);
            string sql = @"SELECT id_inquilino, dni, nombre, apellido,
                                  telefono, correo, direccion
                           FROM Inquilino
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

        private static Inquilino Mapear(MySqlDataReader reader)
        {
            return new Inquilino
            {
                IdInquilino = Convert.ToInt32(reader["id_inquilino"]),
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
