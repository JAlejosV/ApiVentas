using ApiVentas.Modelos.ApiResponse;
using ApiVentas.Modelos.Dtos;
using ApiVentas.Repositorio.IRepositorio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace ApiVentas.Controllers
{
    [Route("api/v1/unidad-medida")]
    [Authorize]
    [ApiController]
    public class UnidadMedidaController : ControllerBase
    {
        private readonly IUnidadMedidaRepositorio _unidadMedidaRepo;

        public UnidadMedidaController(IUnidadMedidaRepositorio unidadMedidaRepo)
        {
            _unidadMedidaRepo = unidadMedidaRepo;
        }

        /// <summary>
        /// Obtener todas las unidades de medida activas
        /// </summary>
        /// <returns>Lista de unidades de medida activas</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<UnidadMedidaDto>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetUnidadesMedida()
        {
            try
            {
                var unidades = await _unidadMedidaRepo.ObtenerActivosAsync();

                var unidadesDto = unidades.Select(u => new UnidadMedidaDto
                {
                    IdUnidadMedida = u.IdUnidadMedida,
                    CodigoUnidadMedida = u.CodigoUnidadMedida,
                    NombreUnidadMedida = u.NombreUnidadMedida,
                    EstadoRegistro = u.EstadoRegistro
                }).ToList();

                var response = ApiResponse<List<UnidadMedidaDto>>.SuccessResponse(
                    unidadesDto,
                    "Unidades de medida obtenidas exitosamente"
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
