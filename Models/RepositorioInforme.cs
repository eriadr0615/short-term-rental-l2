using MySqlConnector;

namespace Inmobiliaria.Models
{
    public class RepositorioInforme : RepositorioBase, IRepositorioInforme
    {
        public RepositorioInforme(IConfiguration configuration) : base(configuration) { }

        public IList<InformeInmueble> InmueblesConPropietario(
            bool? disponible, int pagina, int tamanoPagina, out int totalRegistros)
        {
            return ConsultarInmuebles("(@disponible IS NULL OR i.disponible = @disponible)",
                new Dictionary<string, object?> { ["@disponible"] = disponible },
                pagina, tamanoPagina, out totalRegistros);
        }

        public IList<InformeInmueble> InmueblesPorPropietario(
            string dni, int pagina, int tamanoPagina, out int totalRegistros)
        {
            return ConsultarInmuebles("p.dni = @dni",
                new Dictionary<string, object?> { ["@dni"] = dni.Trim() },
                pagina, tamanoPagina, out totalRegistros);
        }

        public IList<InformeInmueble> InmueblesSinReserva(
            int dias, int pagina, int tamanoPagina, out int totalRegistros)
        {
            // Una estadía iniciada antes del período también cuenta si lo ocupa.
            string filtro = @"NOT EXISTS (
                SELECT 1 FROM Reserva r
                WHERE r.id_inmueble = i.id_inmueble
                  AND r.fecha_inicio <= @hoy
                  AND COALESCE(r.fecha_finalizacion_anticipada, r.fecha_fin_original) > @desde)";
            return ConsultarInmuebles(filtro, new Dictionary<string, object?>
            {
                ["@hoy"] = DateTime.Today,
                ["@desde"] = DateTime.Today.AddDays(-dias)
            }, pagina, tamanoPagina, out totalRegistros);
        }

        public IList<InformeInmueble> InmueblesLibres(
            DateTime fechaInicio, DateTime fechaFin,
            int pagina, int tamanoPagina, out int totalRegistros)
        {
            // Igual criterio que al reservar: la salida permite otra entrada ese mismo día.
            string filtro = @"i.disponible = TRUE AND NOT EXISTS (
                SELECT 1 FROM Reserva r
                WHERE r.id_inmueble = i.id_inmueble
                  AND r.fecha_inicio < @fin
                  AND COALESCE(r.fecha_finalizacion_anticipada, r.fecha_fin_original) > @inicio)";
            return ConsultarInmuebles(filtro, new Dictionary<string, object?>
            {
                ["@inicio"] = fechaInicio.Date,
                ["@fin"] = fechaFin.Date
            }, pagina, tamanoPagina, out totalRegistros);
        }

        public IList<InformeInmueble> InmueblesMasReservados(
            int pagina, int tamanoPagina, out int totalRegistros)
        {
            var lista = new List<InformeInmueble>();
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            var parametros = new Dictionary<string, object?>
            {
                ["@desde"] = DateTime.Today.AddDays(-365),
                ["@hoy"] = DateTime.Today
            };
            const string filtro = "r.fecha_inicio >= @desde AND r.fecha_inicio <= @hoy";
            using (var count = new MySqlCommand(
                $"SELECT COUNT(DISTINCT r.id_inmueble) FROM Reserva r WHERE {filtro}", connection))
            {
                AgregarParametros(count, parametros);
                totalRegistros = Convert.ToInt32(count.ExecuteScalar());
            }

            string sql = $@"SELECT i.id_inmueble, i.direccion_inmueble, t.nombre_tipo,
                                   p.nombre, p.apellido, COUNT(*) AS cantidad_reservas
                            FROM Reserva r
                            INNER JOIN Inmueble i ON r.id_inmueble = i.id_inmueble
                            INNER JOIN Propietario p ON i.id_propietario = p.id_propietario
                            INNER JOIN TipoInmueble t ON i.id_tipo_inmueble = t.id_tipo_inmueble
                            WHERE {filtro}
                            GROUP BY i.id_inmueble, i.direccion_inmueble, t.nombre_tipo, p.nombre, p.apellido
                            ORDER BY cantidad_reservas DESC, i.id_inmueble
                            LIMIT @tamano OFFSET @offset";
            using var command = new MySqlCommand(sql, connection);
            AgregarParametros(command, parametros);
            AgregarPaginado(command, pagina, tamanoPagina);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(new InformeInmueble
                {
                    IdInmueble = Convert.ToInt32(reader["id_inmueble"]),
                    Direccion = reader["direccion_inmueble"].ToString() ?? "",
                    TipoInmueble = reader["nombre_tipo"].ToString() ?? "",
                    Propietario = $"{reader["nombre"]} {reader["apellido"]}",
                    CantidadReservas = Convert.ToInt32(reader["cantidad_reservas"])
                });
            }
            return lista;
        }

        public IList<InformeReserva> ReservasVigentes(
            int pagina, int tamanoPagina, out int totalRegistros)
        {
            return ConsultarReservas(
                "r.fecha_inicio <= @hoy AND COALESCE(r.fecha_finalizacion_anticipada, r.fecha_fin_original) > @hoy",
                new Dictionary<string, object?> { ["@hoy"] = DateTime.Today },
                pagina, tamanoPagina, out totalRegistros);
        }

        public IList<InformeReserva> ReservasPorFinalizar(
            int dias, int pagina, int tamanoPagina, out int totalRegistros)
        {
            return ConsultarReservas(
                "COALESCE(r.fecha_finalizacion_anticipada, r.fecha_fin_original) BETWEEN @hoy AND @hasta",
                new Dictionary<string, object?>
                {
                    ["@hoy"] = DateTime.Today,
                    ["@hasta"] = DateTime.Today.AddDays(dias)
                }, pagina, tamanoPagina, out totalRegistros);
        }

        private IList<InformeInmueble> ConsultarInmuebles(string filtro,
            Dictionary<string, object?> parametros, int pagina, int tamanoPagina, out int totalRegistros)
        {
            // Los fragmentos SQL son internos; los valores del usuario van como parámetros.
            string tablas = @"FROM Inmueble i
                INNER JOIN Propietario p ON i.id_propietario = p.id_propietario
                INNER JOIN TipoInmueble t ON i.id_tipo_inmueble = t.id_tipo_inmueble";
            var lista = new List<InformeInmueble>();
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            using (var count = new MySqlCommand($"SELECT COUNT(*) {tablas} WHERE {filtro}", connection))
            {
                AgregarParametros(count, parametros);
                totalRegistros = Convert.ToInt32(count.ExecuteScalar());
            }
            string sql = $@"SELECT i.id_inmueble, i.direccion_inmueble, i.precio_diario,
                                   i.capacidad_maxima, i.disponible, p.nombre, p.apellido, t.nombre_tipo
                            {tablas} WHERE {filtro}
                            ORDER BY i.direccion_inmueble, i.id_inmueble
                            LIMIT @tamano OFFSET @offset";
            using var command = new MySqlCommand(sql, connection);
            AgregarParametros(command, parametros);
            AgregarPaginado(command, pagina, tamanoPagina);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(new InformeInmueble
                {
                    IdInmueble = Convert.ToInt32(reader["id_inmueble"]),
                    Direccion = reader["direccion_inmueble"].ToString() ?? "",
                    Propietario = $"{reader["nombre"]} {reader["apellido"]}",
                    TipoInmueble = reader["nombre_tipo"].ToString() ?? "",
                    PrecioDiario = Convert.ToDecimal(reader["precio_diario"]),
                    CapacidadMaxima = Convert.ToInt32(reader["capacidad_maxima"]),
                    Disponible = Convert.ToBoolean(reader["disponible"])
                });
            }
            return lista;
        }

        private IList<InformeReserva> ConsultarReservas(string filtro,
            Dictionary<string, object?> parametros, int pagina, int tamanoPagina, out int totalRegistros)
        {
            string tablas = @"FROM Reserva r
                INNER JOIN Inquilino q ON r.id_inquilino = q.id_inquilino
                INNER JOIN Inmueble i ON r.id_inmueble = i.id_inmueble";
            var lista = new List<InformeReserva>();
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            using (var count = new MySqlCommand($"SELECT COUNT(*) {tablas} WHERE {filtro}", connection))
            {
                AgregarParametros(count, parametros);
                totalRegistros = Convert.ToInt32(count.ExecuteScalar());
            }
            string sql = $@"SELECT r.id_reserva, q.nombre, q.apellido, i.direccion_inmueble,
                                   r.fecha_inicio, r.fecha_fin_original, r.fecha_finalizacion_anticipada, r.monto_dia
                            {tablas} WHERE {filtro}
                            ORDER BY COALESCE(r.fecha_finalizacion_anticipada, r.fecha_fin_original), r.id_reserva
                            LIMIT @tamano OFFSET @offset";
            using var command = new MySqlCommand(sql, connection);
            AgregarParametros(command, parametros);
            AgregarPaginado(command, pagina, tamanoPagina);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(new InformeReserva
                {
                    IdReserva = Convert.ToInt32(reader["id_reserva"]),
                    Inquilino = $"{reader["nombre"]} {reader["apellido"]}",
                    Direccion = reader["direccion_inmueble"].ToString() ?? "",
                    FechaInicio = Convert.ToDateTime(reader["fecha_inicio"]),
                    FechaFinOriginal = Convert.ToDateTime(reader["fecha_fin_original"]),
                    FechaFinalizacionAnticipada = reader["fecha_finalizacion_anticipada"] == DBNull.Value
                        ? null : Convert.ToDateTime(reader["fecha_finalizacion_anticipada"]),
                    MontoDia = Convert.ToDecimal(reader["monto_dia"])
                });
            }
            return lista;
        }

        private static void AgregarParametros(MySqlCommand command, Dictionary<string, object?> parametros)
        {
            foreach (var parametro in parametros)
                command.Parameters.AddWithValue(parametro.Key, parametro.Value ?? DBNull.Value);
        }

        private static void AgregarPaginado(MySqlCommand command, int pagina, int tamanoPagina)
        {
            command.Parameters.AddWithValue("@tamano", tamanoPagina);
            command.Parameters.AddWithValue("@offset", (long)(Math.Max(pagina, 1) - 1) * tamanoPagina);
        }
    }
}




