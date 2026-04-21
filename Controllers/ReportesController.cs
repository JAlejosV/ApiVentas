using ApiVentas.Modelos.ApiResponse;
using ApiVentas.Modelos.Dtos;
using ApiVentas.Repositorio.IRepositorio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace ApiVentas.Controllers
{
    [Route("api/v1/reportes")]
    [Authorize]
    [ApiController]
    public class ReportesController : ControllerBase
    {
        private readonly IReporteRepositorio _reporteRepo;

        public ReportesController(IReporteRepositorio reporteRepo)
        {
            _reporteRepo = reporteRepo;
        }

        /// <summary>
        /// Reporte de ventas (Notas de Venta) desde la base de datos externa
        /// </summary>
        /// <param name="fechaInicio">Fecha inicio obligatoria (yyyy-MM-dd). Si fechaFin es nulo, filtra por fecha exacta.</param>
        /// <param name="fechaFin">Fecha fin opcional (yyyy-MM-dd). Si se indica, filtra en el rango [fechaInicio, fechaFin].</param>
        /// <param name="idUsuario">ID del vendedor (opcional). NULL = todos.</param>
        /// <param name="codigoProducto">Código interno del producto (opcional). NULL = todos.</param>
        /// <param name="unidadMedida">Código de unidad de medida, ej. BX, NIU (opcional). NULL = todas.</param>
        [HttpGet("ventas")]
        [ProducesResponseType(typeof(ApiResponse<List<ReporteVentaItemDto>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetReporteVentas(
            [FromQuery] string fechaInicio,
            [FromQuery] string fechaFin       = null,
            [FromQuery] int?   idUsuario      = null,
            [FromQuery] string codigoProducto = null,
            [FromQuery] string unidadMedida   = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fechaInicio))
                {
                    var badRequest = ApiResponse<object>.ErrorResponse(
                        "Parámetro requerido",
                        "El parámetro fechaInicio es obligatorio (formato: yyyy-MM-dd)"
                    );
                    badRequest.Path = Request.Path;
                    return BadRequest(badRequest);
                }

                var items = await _reporteRepo.ObtenerReporteVentasAsync(
                    fechaInicio, fechaFin, idUsuario, codigoProducto, unidadMedida);

                var response = ApiResponse<List<ReporteVentaItemDto>>.SuccessResponse(
                    items,
                    "Reporte de ventas obtenido exitosamente"
                );
                response.Path = Request.Path;

                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.ErrorResponse(
                    "Error interno del servidor",
                    ex.Message
                );
                errorResponse.Path = Request.Path;
                return StatusCode(500, errorResponse);
            }
        }
    }
}

