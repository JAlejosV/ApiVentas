using ApiVentas.Modelos.ApiResponse;
using ApiVentas.Modelos.Dtos;
using ApiVentas.Repositorio.IRepositorio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace ApiVentas.Controllers
{
    [Route("api/v1/usuarios")]
    [Authorize]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuarioSistemaRepositorio _usuarioSistemaRepo;

        public UsuariosController(IUsuarioSistemaRepositorio usuarioSistemaRepo)
        {
            _usuarioSistemaRepo = usuarioSistemaRepo;
        }

        /// <summary>
        /// Obtener todos los usuarios activos
        /// </summary>
        /// <returns>Lista de usuarios activos</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<UsuarioListaDto>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetUsuarios()
        {
            try
            {
                var usuarios = await _usuarioSistemaRepo.ObtenerUsuariosActivosAsync();

                var usuariosDto = usuarios.Select(u => new UsuarioListaDto
                {
                    IdUsuario = u.IdUsuario,
                    CodigoEquivalencia = u.CodigoEquivalencia,
                    NombreCompleto = u.NombreCompleto,
                    Correo = u.Correo,
                    EstadoRegistro = u.EstadoRegistro,
                    FechaCreacion = u.FechaCreacion
                }).ToList();

                var response = ApiResponse<List<UsuarioListaDto>>.SuccessResponse(
                    usuariosDto,
                    "Usuarios obtenidos exitosamente"
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
