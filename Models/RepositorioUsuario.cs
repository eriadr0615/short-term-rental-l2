using MySqlConnector;

namespace Inmobiliaria.Models
{
    public class RepositorioUsuario : RepositorioBase, IRepositorioUsuario
    {
        public RepositorioUsuario(IConfiguration configuration)
            : base(configuration)
        {
        }

        public int Alta(Usuario usuario)
        {
            using var connection = new MySqlConnection(connectionString);
            string sql = @"INSERT INTO Usuario
                           (avatar, nombre_usuario, correo_usuario, contrasenia_hash,
                            rol_usuario, activo)
                           VALUES
                           (@avatar, @nombre, @correo, @hash, @rol, @activo);
                           SELECT LAST_INSERT_ID();";

            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@avatar", (object?)usuario.Avatar ?? DBNull.Value);
            command.Parameters.AddWithValue("@nombre", usuario.NombreUsuario);
            command.Parameters.AddWithValue("@correo", usuario.CorreoUsuario);
            command.Parameters.AddWithValue("@hash", usuario.ContraseniaHash);
            command.Parameters.AddWithValue("@rol", usuario.RolUsuario);
            command.Parameters.AddWithValue("@activo", usuario.Activo);

            connection.Open();
            usuario.IdUsuario = Convert.ToInt32(command.ExecuteScalar());
            return usuario.IdUsuario;
        }

        public int Baja(int id)
        {
            return CambiarEstado(id, false);
        }

        public int Modificacion(Usuario usuario)
        {
            using var connection = new MySqlConnection(connectionString);
            string sql = @"UPDATE Usuario
                           SET nombre_usuario = @nombre,
                               correo_usuario = @correo,
                               rol_usuario = @rol,
                               activo = @activo
                           WHERE id_usuario = @id";

            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@nombre", usuario.NombreUsuario);
            command.Parameters.AddWithValue("@correo", usuario.CorreoUsuario);
            command.Parameters.AddWithValue("@rol", usuario.RolUsuario);
            command.Parameters.AddWithValue("@activo", usuario.Activo);
            command.Parameters.AddWithValue("@id", usuario.IdUsuario);

            connection.Open();
            return command.ExecuteNonQuery();
        }

        public IList<Usuario> ObtenerLista()
        {
            var lista = new List<Usuario>();
            using var connection = new MySqlConnection(connectionString);
            string sql = @"SELECT id_usuario, avatar, nombre_usuario, correo_usuario,
                                  contrasenia_hash, rol_usuario, activo
                           FROM Usuario
                           ORDER BY nombre_usuario";

            using var command = new MySqlCommand(sql, connection);
            connection.Open();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(Mapear(reader));
            }

            return lista;
        }

        public Usuario? ObtenerPorId(int id)
        {
            using var connection = new MySqlConnection(connectionString);
            string sql = @"SELECT id_usuario, avatar, nombre_usuario, correo_usuario,
                                  contrasenia_hash, rol_usuario, activo
                           FROM Usuario
                           WHERE id_usuario = @id";

            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@id", id);
            connection.Open();
            using var reader = command.ExecuteReader();
            return reader.Read() ? Mapear(reader) : null;
        }

        public Usuario? ObtenerPorCorreo(string correo)
        {
            using var connection = new MySqlConnection(connectionString);
            string sql = @"SELECT id_usuario, avatar, nombre_usuario, correo_usuario,
                                  contrasenia_hash, rol_usuario, activo
                           FROM Usuario
                           WHERE correo_usuario = @correo";

            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@correo", correo);
            connection.Open();
            using var reader = command.ExecuteReader();
            return reader.Read() ? Mapear(reader) : null;
        }

        public int ActualizarClave(int id, string contraseniaHash)
        {
            using var connection = new MySqlConnection(connectionString);
            string sql = @"UPDATE Usuario
                           SET contrasenia_hash = @hash
                           WHERE id_usuario = @id";
            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@hash", contraseniaHash);
            command.Parameters.AddWithValue("@id", id);
            connection.Open();
            return command.ExecuteNonQuery();
        }

        public int ActualizarAvatar(int id, string? avatar)
        {
            using var connection = new MySqlConnection(connectionString);
            string sql = @"UPDATE Usuario
                           SET avatar = @avatar
                           WHERE id_usuario = @id";
            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@avatar", (object?)avatar ?? DBNull.Value);
            command.Parameters.AddWithValue("@id", id);
            connection.Open();
            return command.ExecuteNonQuery();
        }

        public int CambiarEstado(int id, bool activo)
        {
            using var connection = new MySqlConnection(connectionString);
            string sql = @"UPDATE Usuario
                           SET activo = @activo
                           WHERE id_usuario = @id";
            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@activo", activo);
            command.Parameters.AddWithValue("@id", id);
            connection.Open();
            return command.ExecuteNonQuery();
        }

        public IList<Usuario> ObtenerLista(
            int pagina,
            int tamanoPagina,
            string? buscar,
            out int totalRegistros)
        {
            var lista = new List<Usuario>();
            string termino = buscar?.Trim() ?? "";
            int offset = (pagina - 1) * tamanoPagina;

            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            string filtro = @"WHERE @buscar = ''
                              OR nombre_usuario LIKE @patron
                              OR correo_usuario LIKE @patron
                              OR rol_usuario LIKE @patron";
            using (var count = new MySqlCommand(
                $"SELECT COUNT(*) FROM Usuario {filtro}", connection))
            {
                count.Parameters.AddWithValue("@buscar", termino);
                count.Parameters.AddWithValue("@patron", $"%{termino}%");
                totalRegistros = Convert.ToInt32(count.ExecuteScalar());
            }

            string sql = $@"SELECT id_usuario, avatar, nombre_usuario, correo_usuario,
                                   contrasenia_hash, rol_usuario, activo
                            FROM Usuario
                            {filtro}
                            ORDER BY nombre_usuario
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

        private static Usuario Mapear(MySqlDataReader reader)
        {
            return new Usuario
            {
                IdUsuario = Convert.ToInt32(reader["id_usuario"]),
                Avatar = reader["avatar"] == DBNull.Value ? null : reader["avatar"].ToString(),
                NombreUsuario = reader["nombre_usuario"].ToString() ?? "",
                CorreoUsuario = reader["correo_usuario"].ToString() ?? "",
                ContraseniaHash = reader["contrasenia_hash"].ToString() ?? "",
                RolUsuario = reader["rol_usuario"].ToString() ?? Usuario.RolEmpleado,
                Activo = Convert.ToBoolean(reader["activo"])
            };
        }
    }
}
