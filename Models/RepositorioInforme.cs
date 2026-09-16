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
