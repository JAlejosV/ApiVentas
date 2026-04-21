using ApiVentas.Modelos.Dtos;
using ApiVentas.Repositorio.IRepositorio;
using ApiVentas.Services;
using Microsoft.Extensions.Options;
using MySqlConnector;
using Renci.SshNet;

namespace ApiVentas.Repositorio
{
    public class ReporteRepositorio : IReporteRepositorio
    {
        private readonly MariaDbSettings _settings;

        public ReporteRepositorio(IOptions<MariaDbSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task<List<ReporteVentaItemDto>> ObtenerReporteVentasAsync(
            string fechaInicio,
            string fechaFin,
            int? idUsuario,
            string codigoProducto,
            string unidadMedida)
        {
            var resultado = new List<ReporteVentaItemDto>();

            SshClient sshClient = null;
            ForwardedPortLocal portForward = null;
            MySqlConnection connection = null;

            try
            {
                // Abrir túnel SSH
                sshClient = new SshClient(
                    _settings.SSH.Host,
                    _settings.SSH.Port,
                    _settings.SSH.User,
                    _settings.SSH.Password
                );
                sshClient.Connect();

                // Reenviar puerto local (0 = OS asigna puerto disponible) → DB remota
                portForward = new ForwardedPortLocal(
                    "127.0.0.1", 0,
                    _settings.Database.Host, (uint)_settings.Database.Port
                );
                sshClient.AddForwardedPort(portForward);
                portForward.Start();

                var connStr = new MySqlConnectionStringBuilder
                {
                    Server   = "127.0.0.1",
                    Port     = portForward.BoundPort,
                    Database = _settings.Database.Name,
                    UserID   = _settings.Database.User,
                    Password = _settings.Database.Password
                }.ToString();

                connection = new MySqlConnection(connStr);
                await connection.OpenAsync();

                const string sql = @"
SELECT
    sn.date_of_issue                                                        AS FechaEmision,
    'Nota de Venta'                                                         AS TipoDocumento,
    sn.series                                                               AS Serie,
    sn.number                                                               AS NumeroDocumento,
    j.CodigoProducto,
    j.Producto,
    -- ✅ UnidadMedida: ""BX - Caja (12)"" si no es NIU ni ZZ, sino ""NIU - Servicio""
    CASE
        WHEN j.UnidadMedida NOT IN ('NIU', 'ZZ')
        THEN CONCAT(
                j.UnidadMedida, ' - ', cut.description,
                ' (', CAST(j.UnidadesPorPresentacion AS CHAR), ')'
             )
        ELSE CONCAT(j.UnidadMedida, ' - ', cut.description)
    END                                                                     AS UnidadMedida,
    sni.quantity                                                            AS CantidadVenta,
    j.UnidadesPorPresentacion,
    sn.currency_type_id                                                     AS Moneda,
    j.PrecioCompra,
    sni.unit_price                                                          AS PrecioUnitario,
    CASE
        WHEN j.UnidadMedida NOT IN ('NIU', 'ZZ')
        THEN sni.quantity * j.UnidadesPorPresentacion
        ELSE sni.quantity
    END                                                                     AS CantidadPorUnidades,
    sni.total - (sni.quantity * j.PrecioCompra)                             AS Ganancia,
    sni.total                                                               AS Total,
    u.id                                                                    AS IdVendedor,
    u.name                                                                  AS Vendedor

FROM sale_notes sn
JOIN sale_note_items sni ON sn.id = sni.sale_note_id

JOIN (
    SELECT
        id,
        JSON_UNQUOTE(JSON_EXTRACT(item, '$.internal_id'))                   AS CodigoProducto,
        JSON_UNQUOTE(JSON_EXTRACT(item, '$.name'))                          AS Producto,
        JSON_UNQUOTE(JSON_EXTRACT(item, '$.unit_type_id'))                  AS UnidadMedida,
        IFNULL(CAST(JSON_UNQUOTE(JSON_EXTRACT(item, '$.presentation.quantity_unit'))
            AS DECIMAL(10,0)), 1)                                           AS UnidadesPorPresentacion,
        CASE
            WHEN JSON_UNQUOTE(JSON_EXTRACT(item, '$.unit_type_id')) NOT IN ('NIU', 'ZZ')
            THEN CAST(JSON_UNQUOTE(JSON_EXTRACT(item, '$.purchase_unit_price'))
                     AS DECIMAL(10,4))
                 * IFNULL(CAST(JSON_UNQUOTE(JSON_EXTRACT(item, '$.presentation.quantity_unit'))
                     AS DECIMAL(10,0)), 1)
            ELSE CAST(JSON_UNQUOTE(JSON_EXTRACT(item, '$.purchase_unit_price'))
                     AS DECIMAL(10,4))
        END                                                                 AS PrecioCompra
    FROM sale_note_items
) j ON j.id = sni.id

-- ✅ JOIN con cat_unit_types por j.UnidadMedida
JOIN cat_unit_types cut ON cut.id = j.UnidadMedida

JOIN state_types    st   ON sn.state_type_id = st.id
JOIN cash_documents cdoc ON sn.id            = cdoc.sale_note_id
JOIN cash           c    ON cdoc.cash_id     = c.id
JOIN users          u    ON c.user_id        = u.id

WHERE st.id NOT IN ('09', '11', '13')
  AND (
      CASE
          WHEN @FechaFin IS NULL
          THEN sn.date_of_issue = @FechaInicio
          ELSE sn.date_of_issue BETWEEN @FechaInicio AND @FechaFin
      END
  )
  AND (@IdUsuario      IS NULL OR u.id             = @IdUsuario)
  AND (@CodigoProducto IS NULL OR j.CodigoProducto = @CodigoProducto)
  AND (@UnidadMedida   IS NULL OR j.UnidadMedida   = @UnidadMedida)

ORDER BY sni.sale_note_id";

                using var cmd = new MySqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("@FechaInicio", fechaInicio);
                cmd.Parameters.AddWithValue("@FechaFin",       string.IsNullOrEmpty(fechaFin)       ? DBNull.Value : fechaFin);
                cmd.Parameters.AddWithValue("@IdUsuario",      idUsuario.HasValue                   ? idUsuario.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@CodigoProducto", string.IsNullOrEmpty(codigoProducto) ? DBNull.Value : codigoProducto);
                cmd.Parameters.AddWithValue("@UnidadMedida",   string.IsNullOrEmpty(unidadMedida)   ? DBNull.Value : unidadMedida);

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    resultado.Add(new ReporteVentaItemDto
                    {
                        FechaEmision            = reader.GetDateTime("FechaEmision").ToString("yyyy-MM-dd"),
                        TipoDocumento           = Convert.ToString(reader["TipoDocumento"]),
                        Serie                   = Convert.ToString(reader["Serie"]),
                        NumeroDocumento         = Convert.ToString(reader["NumeroDocumento"]),
                        CodigoProducto          = reader.IsDBNull(reader.GetOrdinal("CodigoProducto"))   ? null : Convert.ToString(reader["CodigoProducto"]),
                        Producto                = reader.IsDBNull(reader.GetOrdinal("Producto"))          ? null : Convert.ToString(reader["Producto"]),
                        UnidadMedida            = reader.IsDBNull(reader.GetOrdinal("UnidadMedida"))      ? null : Convert.ToString(reader["UnidadMedida"]),
                        CantidadVenta           = reader.GetDecimal("CantidadVenta"),
                        UnidadesPorPresentacion = reader.GetDecimal("UnidadesPorPresentacion"),
                        Moneda                  = reader.IsDBNull(reader.GetOrdinal("Moneda"))            ? null : Convert.ToString(reader["Moneda"]),
                        PrecioCompra            = reader.GetDecimal("PrecioCompra"),
                        PrecioUnitario          = reader.GetDecimal("PrecioUnitario"),
                        CantidadPorUnidades     = reader.GetDecimal("CantidadPorUnidades"),
                        Ganancia                = reader.GetDecimal("Ganancia"),
                        Total                   = reader.GetDecimal("Total"),
                        IdVendedor              = reader.GetInt32("IdVendedor"),
                        Vendedor                = reader.IsDBNull(reader.GetOrdinal("Vendedor"))          ? null : Convert.ToString(reader["Vendedor"])
                    });
                }
            }
            finally
            {
                if (connection != null) await connection.DisposeAsync();
                portForward?.Stop();
                sshClient?.Disconnect();
                sshClient?.Dispose();
            }

            return resultado;
        }
    }
}
