using MySqlConnector;

namespace Inmobiliaria.Models
{
    public class RepositorioInforme : RepositorioBase, IRepositorioInforme
    {
        public RepositorioInforme(IConfiguration configuration)
            : base(configuration)
        {
        }
        public IList<InformeInmueble> InmueblesConPropietario(
            bool? disponible,
            int pagina,
            int tamanoPagina,
            out int totalRegistros)
        {
            IList<InformeInmueble> lista = new List<InformeInmueble>();
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                using (var count = new MySqlCommand(
                    @"SELECT COUNT(*)
                      FROM Inmueble
                      WHERE (@disponible IS NULL OR disponible = @disponible)",
                    connection))
                {
                    count.Parameters.AddWithValue(
                        "@disponible", (object?)disponible ?? DBNull.Value);
                    totalRegistros = Convert.ToInt32(count.ExecuteScalar());
                }

                string sql = @"
                    SELECT
                        i.id_inmueble,
                        i.direccion_inmueble,
                        i.precio_diario,
                        i.capacidad_maxima,
                        i.disponible,
                        p.nombre,
                        p.apellido,
                        t.nombre_tipo
                    FROM Inmueble i
                    INNER JOIN Propietario p
                        ON i.id_propietario = p.id_propietario
                    INNER JOIN TipoInmueble t
                        ON i.id_tipo_inmueble = t.id_tipo_inmueble
                    WHERE (@disponible IS NULL
                           OR i.disponible = @disponible)
                    ORDER BY i.direccion_inmueble
                    LIMIT @tamano OFFSET @offset;";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue(
                        "@disponible",
                        (object?)disponible ?? DBNull.Value
                    );
                    command.Parameters.AddWithValue("@tamano", tamanoPagina);
                    command.Parameters.AddWithValue(
                        "@offset", (pagina - 1) * tamanoPagina);

                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        InformeInmueble informe = new InformeInmueble
                        {
                            IdInmueble =
                                Convert.ToInt32(reader["id_inmueble"]),
                            Direccion =
                                reader["direccion_inmueble"].ToString() ?? "",
                            Propietario =
                                $"{reader["nombre"]} {reader["apellido"]}",
                            TipoInmueble =
                                reader["nombre_tipo"].ToString() ?? "",
                            PrecioDiario =
                                Convert.ToDecimal(reader["precio_diario"]),
                            CapacidadMaxima =
                                Convert.ToInt32(reader["capacidad_maxima"]),
                            Disponible =
                                Convert.ToBoolean(reader["disponible"])
                        };
                        lista.Add(informe);
                    }
                }
            }
            return lista;
        }


        public IList<InformeReserva> ReservasPorFinalizar(
            int dias,
            int pagina,
            int tamanoPagina,
            out int totalRegistros)
        {
            IList<InformeReserva> lista =
                new List<InformeReserva>();

            totalRegistros = 0;

            using (var connection =
                new MySqlConnection(connectionString))
            {
                connection.Open();

                string sqlCantidad = @"
                    SELECT COUNT(*)
                    FROM Reserva r
                    WHERE COALESCE(
                        r.fecha_finalizacion_anticipada,
                        r.fecha_fin_original
                    ) BETWEEN CURDATE()
                    AND DATE_ADD(CURDATE(), INTERVAL @dias DAY);";

                using (var commandCantidad =
                    new MySqlCommand(sqlCantidad, connection))
                {
                    commandCantidad.Parameters.AddWithValue(
                        "@dias", dias);

                    totalRegistros =
                        Convert.ToInt32(
                            commandCantidad.ExecuteScalar());
                }

                int offset =
                    (pagina - 1) * tamanoPagina;
                string sql = @"
                    SELECT
                        r.id_reserva,
                        r.fecha_inicio,
                        r.fecha_fin_original,
                        r.fecha_finalizacion_anticipada,
                        r.monto_dia,
                        iq.nombre AS nombre_inquilino,
                        iq.apellido AS apellido_inquilino,
                        i.direccion_inmueble
                    FROM Reserva r

                    INNER JOIN Inquilino iq
                        ON r.id_inquilino = iq.id_inquilino

                    INNER JOIN Inmueble i
                        ON r.id_inmueble = i.id_inmueble

                    WHERE COALESCE(
                        r.fecha_finalizacion_anticipada,
                        r.fecha_fin_original
                    ) BETWEEN CURDATE()
                    AND DATE_ADD(CURDATE(), INTERVAL @dias DAY)

                    ORDER BY COALESCE(
                        r.fecha_finalizacion_anticipada,
                        r.fecha_fin_original
                    )
                    LIMIT @limite OFFSET @offset;";

                using (var command =
                    new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue(
                        "@dias", dias);

                    command.Parameters.AddWithValue(
                        "@limite", tamanoPagina);
                    command.Parameters.AddWithValue(
                        "@offset", offset);
                    using (var reader =
                        command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            InformeReserva informe =
                                new InformeReserva
                                {
                                    IdReserva =
                                        Convert.ToInt32(
                                            reader["id_reserva"]),

                                    Inquilino =
                                        $"{reader["nombre_inquilino"]} " +
                                        $"{reader["apellido_inquilino"]}",

                                    Inmueble =
                                        reader["direccion_inmueble"]
                                        .ToString() ?? "",

                                    FechaInicio =
                                        Convert.ToDateTime(
                                            reader["fecha_inicio"]),

                                    FechaFin =
                                        reader["fecha_finalizacion_anticipada"]
                                            != DBNull.Value
                                            ? Convert.ToDateTime(
                                                reader["fecha_finalizacion_anticipada"])
                                            : Convert.ToDateTime(
                                                reader["fecha_fin_original"]),
                                    MontoDia =
                                        Convert.ToDecimal(
                                            reader["monto_dia"])
                                };
                            lista.Add(informe);
                        }
                    }
                }
            }

            return lista;
        }




        public IList<InformeReserva> ReservasVigentes(
            int pagina,
            int tamanoPagina,
            out int totalRegistros)
        {
            IList<InformeReserva> lista =
                new List<InformeReserva>();

            totalRegistros = 0;

            using (var connection =
                new MySqlConnection(connectionString))
            {
                connection.Open();
                string sqlCantidad = @"
            SELECT COUNT(*)
            FROM Reserva r
            WHERE r.fecha_inicio <= CURDATE()
            AND COALESCE(
                r.fecha_finalizacion_anticipada,
                r.fecha_fin_original
            ) >= CURDATE();";

                using (var commandCantidad =
                    new MySqlCommand(sqlCantidad, connection))
                {
                    totalRegistros =
                        Convert.ToInt32(
                            commandCantidad.ExecuteScalar());
                }
                int offset =
                    (pagina - 1) * tamanoPagina;

                string sql = @"
            SELECT
                r.id_reserva,
                r.fecha_inicio,
                r.fecha_fin_original,
                r.fecha_finalizacion_anticipada,
                r.monto_dia,
                iq.nombre AS nombre_inquilino,
                iq.apellido AS apellido_inquilino,
                i.direccion_inmueble
            FROM Reserva r

            INNER JOIN Inquilino iq
                ON r.id_inquilino = iq.id_inquilino

            INNER JOIN Inmueble i
                ON r.id_inmueble = i.id_inmueble

            WHERE r.fecha_inicio <= CURDATE()

            AND COALESCE(
                r.fecha_finalizacion_anticipada,
                r.fecha_fin_original
            ) >= CURDATE()

            ORDER BY r.fecha_fin_original

            LIMIT @limite OFFSET @offset;";

                using (var command =
                    new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue(
                        "@limite", tamanoPagina);
                    command.Parameters.AddWithValue(
                        "@offset", offset);
                    using (var reader =
                        command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            InformeReserva informe =
                                new InformeReserva
                                {
                                    IdReserva =
                                        Convert.ToInt32(
                                            reader["id_reserva"]),

                                    Inquilino =
                                        $"{reader["nombre_inquilino"]} " +
                                        $"{reader["apellido_inquilino"]}",

                                    Inmueble =
                                        reader["direccion_inmueble"]
                                        .ToString() ?? "",

                                    FechaInicio =
                                        Convert.ToDateTime(
                                            reader["fecha_inicio"]),

                                    FechaFin =
                                        reader["fecha_finalizacion_anticipada"]
                                            != DBNull.Value
                                            ? Convert.ToDateTime(
                                                reader["fecha_finalizacion_anticipada"])
                                            : Convert.ToDateTime(
                                                reader["fecha_fin_original"]),

                                    MontoDia =
                                        Convert.ToDecimal(
                                            reader["monto_dia"])
                                };

                            lista.Add(informe);
                        }
                    }
                }
            }
            return lista;
        }


        public IList<InformeInmueble> InmueblesMasReservados(
            int pagina,
            int tamanoPagina,
            out int totalRegistros)
        {
            IList<InformeInmueble> lista = new List<InformeInmueble>();
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                using (var count = new MySqlCommand(
                    @"SELECT COUNT(DISTINCT id_inmueble)
                      FROM Reserva
                      WHERE fecha_inicio >= DATE_SUB(CURDATE(), INTERVAL 365 DAY)
                        AND fecha_inicio <= CURDATE()", connection))
                {
                    totalRegistros = Convert.ToInt32(count.ExecuteScalar());
                }

                string sql = @"
                    SELECT
                        i.id_inmueble,
                        i.direccion_inmueble,
                        t.nombre_tipo,
                        p.nombre,
                        p.apellido,
                        COUNT(r.id_reserva) AS cantidad_reservas
                    FROM Reserva r
                    INNER JOIN Inmueble i
                        ON r.id_inmueble = i.id_inmueble
                    INNER JOIN Propietario p
                        ON i.id_propietario = p.id_propietario
                    INNER JOIN TipoInmueble t
                        ON i.id_tipo_inmueble = t.id_tipo_inmueble
                    WHERE r.fecha_inicio >= DATE_SUB(CURDATE(), INTERVAL 365 DAY)
                    AND r.fecha_inicio <= CURDATE()
                    GROUP BY
                        i.id_inmueble,
                        i.direccion_inmueble,
                        t.nombre_tipo,
                        p.nombre,
                        p.apellido
                    ORDER BY cantidad_reservas DESC
                    LIMIT @tamano OFFSET @offset;";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@tamano", tamanoPagina);
                    command.Parameters.AddWithValue(
                        "@offset", (pagina - 1) * tamanoPagina);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            InformeInmueble informe = new InformeInmueble
                            {
                                IdInmueble =
                                    Convert.ToInt32(reader["id_inmueble"]),
                                Direccion =
                                    reader["direccion_inmueble"].ToString() ?? "",
                                TipoInmueble =
                                    reader["nombre_tipo"].ToString() ?? "",
                                Propietario =
                                    $"{reader["nombre"]} {reader["apellido"]}",
                                CantidadReservas =
                                    Convert.ToInt32(reader["cantidad_reservas"])
                            };
                            lista.Add(informe);
                        }
                    }
                }
            }
            return lista;
        }

        public IList<InformeInmueble> InmueblesSinReserva(int dias)
        {
            IList<InformeInmueble> lista = new List<InformeInmueble>();

            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"
            SELECT
                i.id_inmueble,
                i.direccion_inmueble,
                i.precio_diario,
                i.capacidad_maxima,
                i.disponible,
                p.nombre,
                p.apellido,
                t.nombre_tipo
            FROM Inmueble i
            INNER JOIN Propietario p
                ON i.id_propietario = p.id_propietario
            INNER JOIN TipoInmueble t
                ON i.id_tipo_inmueble = t.id_tipo_inmueble
            WHERE NOT EXISTS
            (
                SELECT 1
                FROM Reserva r
                WHERE r.id_inmueble = i.id_inmueble
                AND r.fecha_inicio >= DATE_SUB(CURDATE(), INTERVAL @dias DAY)
                AND r.fecha_inicio <= CURDATE()
            )
            ORDER BY i.direccion_inmueble";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@dias", dias);

                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            InformeInmueble informe = new InformeInmueble
                            {
                                IdInmueble =
                                    Convert.ToInt32(reader["id_inmueble"]),

                                Direccion =
                                    reader["direccion_inmueble"].ToString() ?? "",

                                Propietario =
                                    $"{reader["nombre"]} {reader["apellido"]}",

                                TipoInmueble =
                                    reader["nombre_tipo"].ToString() ?? "",

                                PrecioDiario =
                                    Convert.ToDecimal(reader["precio_diario"]),

                                CapacidadMaxima =
                                    Convert.ToInt32(reader["capacidad_maxima"]),

                                Disponible =
                                    Convert.ToBoolean(reader["disponible"])
                            };

                            lista.Add(informe);
                        }
                    }
                }
            }

            return lista;
        }


        public IList<InformeInmueble> InmueblesPorPropietario(string dni)
        {
            IList<InformeInmueble> lista = new List<InformeInmueble>();
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"
                        SELECT
                            i.id_inmueble,
                        
                            i.direccion_inmueble,
                            i.precio_diario,
                            i.capacidad_maxima,
                            i.disponible,
                            p.nombre,
                            p.apellido,
                            t.nombre_tipo
                        FROM Inmueble i
                        INNER JOIN Propietario p
                            ON i.id_propietario = p.id_propietario
                        INNER JOIN TipoInmueble t
                            ON i.id_tipo_inmueble = t.id_tipo_inmueble
                        WHERE p.dni = @dni
                        ORDER BY i.direccion_inmueble";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@dni", dni);
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            InformeInmueble informe = new InformeInmueble
                            {
                                IdInmueble =
                                    Convert.ToInt32(reader["id_inmueble"]),
                                Direccion =
                                    reader["direccion_inmueble"].ToString() ?? "",
                                Propietario =
                                    $"{reader["nombre"]} {reader["apellido"]}",
                                TipoInmueble =
                                    reader["nombre_tipo"].ToString() ?? "",
                                PrecioDiario =
                                    Convert.ToDecimal(reader["precio_diario"]),
                                CapacidadMaxima =
                                    Convert.ToInt32(reader["capacidad_maxima"]),
                                Disponible =
                                    Convert.ToBoolean(reader["disponible"])
                            };

                            lista.Add(informe);
                        }
                    }
                }
            }
            return lista;
        }
    }


}




