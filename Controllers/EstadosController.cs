using ApiVentas.Modelos;
using ApiVentas.Modelos.ApiResponse;
using ApiVentas.Modelos.Dtos;
using ApiVentas.Repositorio.IRepositorio;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace ApiVentas.Controllers
{
    [Route("api/v1/estados")]
    [Authorize]
    [ApiController]
    public class EstadosController : ControllerBase
    {
        private readonly IEstadoRepositorio _estadoRepo;
        private readonly IMapper _mapper;

        public EstadosController(IEstadoRepositorio estadoRepo, IMapper mapper)
        {
            _estadoRepo = estadoRepo;
            _mapper = mapper;
        }

        /// <summary>
        /// Obtener todos los estados con paginación
        /// </summary>
        /// <param name="page">Número de página (por defecto 1)</param>
        /// <param name="pageSize">Tamaño de página (por defecto 10, máximo 100)</param>
        /// <param name="search">Término de búsqueda opcional</param>
        /// <param name="includeInactive">Incluir estados inactivos (por defecto false)</param>
        /// <returns>Lista paginada de estados</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<EstadoDto>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.InternalServerError)]
        public IActionResult GetEstados(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string search = null,
            [FromQuery] bool includeInactive = false)
        {
            try
            {
                // Validaciones
                if (page < 1)
                {
                    var badRequestResponse = ApiResponse<object>.ErrorResponse(
                        "Parámetro inválido",
                        "El número de página debe ser mayor a 0"
                    );
                    badRequestResponse.Path = Request.Path;
                    return BadRequest(badRequestResponse);
                }

                if (pageSize < 1 || pageSize > 100)
                {
                    var badRequestResponse = ApiResponse<object>.ErrorResponse(
                        "Parámetro inválido",
                        "El tamaño de página debe estar entre 1 y 100"
                    );
                    badRequestResponse.Path = Request.Path;
                    return BadRequest(badRequestResponse);
                }

                // Obtener datos
                var listaEstados = _estadoRepo.GetEstados();

                // Aplicar filtros
                if (!includeInactive)
                {
                    listaEstados = listaEstados.Where(e => e.EstadoRegistro).ToList();
                }

                if (!string.IsNullOrWhiteSpace(search))
                {
                    listaEstados = listaEstados.Where(e => 
                        e.NombreEstado.ToLower().Contains(search.ToLower()) ||
                        e.CodigoEstado.ToLower().Contains(search.ToLower()) ||
                        e.DescripcionEstado.ToLower().Contains(search.ToLower())
                    ).ToList();
                }

                // Calcular totales
                var totalRecords = listaEstados.Count();

                // Aplicar paginación
                var estadosPaginados = listaEstados
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                // Mapear a DTOs
                var estadosDto = estadosPaginados.Select(e => _mapper.Map<EstadoDto>(e)).ToList();

                // Crear respuesta paginada
                var paginatedData = new PaginatedResponse<EstadoDto>(estadosDto, page, pageSize, totalRecords);

                var response = ApiResponse<PaginatedResponse<EstadoDto>>.SuccessResponse(
                    paginatedData,
                    "Estados obtenidos exitosamente"
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

        /// <summary>
        /// Obtener un estado específico por ID
        /// </summary>
        /// <param name="idEstado">ID del estado</param>
        /// <returns>Información detallada del estado</returns>
        [HttpGet("{idEstado:int}", Name = "GetEstado")]
        [ProducesResponseType(typeof(ApiResponse<EstadoDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.InternalServerError)]
        public IActionResult GetEstado(int idEstado)
        {
            try
            {
                if (idEstado <= 0)
                {
                    var badRequestResponse = ApiResponse<object>.ErrorResponse(
                        "ID inválido",
                        "El ID del estado debe ser mayor a 0"
                    );
                    badRequestResponse.Path = Request.Path;
                    return BadRequest(badRequestResponse);
                }

                var estado = _estadoRepo.GetEstado(idEstado);

                if (estado == null)
                {
                    var notFoundResponse = ApiResponse<object>.ErrorResponse(
                        "Estado no encontrado",
                        $"No se encontró un estado con el ID {idEstado}"
                    );
                    notFoundResponse.Path = Request.Path;
                    return NotFound(notFoundResponse);
                }

                var estadoDto = _mapper.Map<EstadoDto>(estado);

                var response = ApiResponse<EstadoDto>.SuccessResponse(
                    estadoDto,
                    "Estado obtenido exitosamente"
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

        /// <summary>
        /// Crear un nuevo estado
        /// </summary>
        /// <param name="estadoCrearDto">Datos del estado a crear</param>
        /// <returns>Estado creado</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<EstadoDto>), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.InternalServerError)]
        public IActionResult CrearEstado([FromBody] EstadoCrearDto estadoCrearDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    var validationResponse = ApiResponse<object>.ErrorResponse(
                        "Error de validación",
                        errors
                    );
                    validationResponse.Path = Request.Path;
                    return BadRequest(validationResponse);
                }

                if (estadoCrearDto == null)
                {
                    var badRequestResponse = ApiResponse<object>.ErrorResponse(
                        "Datos inválidos",
                        "Los datos del estado son requeridos"
                    );
                    badRequestResponse.Path = Request.Path;
                    return BadRequest(badRequestResponse);
                }

                // Verificar si ya existe un estado con el mismo nombre
                if (_estadoRepo.ExisteEstado(estadoCrearDto.NombreEstado))
                {
                    var conflictResponse = ApiResponse<object>.ErrorResponse(
                        "Conflicto de datos",
                        $"Ya existe un estado con el nombre '{estadoCrearDto.NombreEstado}'"
                    );
                    conflictResponse.Path = Request.Path;
                    return Conflict(conflictResponse);
                }

                // Verificar si ya existe un estado con el mismo código
                if (_estadoRepo.ExisteCodigoEstado(estadoCrearDto.CodigoEstado))
                {
                    var conflictResponse = ApiResponse<object>.ErrorResponse(
                        "Conflicto de datos",
                        $"Ya existe un estado con el código '{estadoCrearDto.CodigoEstado}'"
                    );
                    conflictResponse.Path = Request.Path;
                    return Conflict(conflictResponse);
                }

                var estado = _mapper.Map<Estado>(estadoCrearDto);

                if (!_estadoRepo.CrearEstado(estado))
                {
                    var errorResponse = ApiResponse<object>.ErrorResponse(
                        "Error al crear estado",
                        "No se pudo crear el estado en la base de datos"
                    );
                    errorResponse.Path = Request.Path;
                    return StatusCode(500, errorResponse);
                }

                var estadoDto = _mapper.Map<EstadoDto>(estado);

                var response = ApiResponse<EstadoDto>.SuccessResponse(
                    estadoDto,
                    "Estado creado exitosamente"
                );
                response.Path = Request.Path;

                return CreatedAtRoute("GetEstado", new { idEstado = estado.IdEstado }, response);
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

        /// <summary>
        /// Actualizar un estado existente
        /// </summary>
        /// <param name="idEstado">ID del estado a actualizar</param>
        /// <param name="estadoActualizarDto">Datos actualizados del estado</param>
        /// <returns>Estado actualizado</returns>
        [HttpPatch("{idEstado:int}", Name = "ActualizarEstado")]
        [ProducesResponseType(typeof(ApiResponse<EstadoDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.InternalServerError)]
        public IActionResult ActualizarEstado(int idEstado, [FromBody] EstadoActualizarDto estadoActualizarDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    var validationResponse = ApiResponse<object>.ErrorResponse(
                        "Error de validación",
                        errors
                    );
                    validationResponse.Path = Request.Path;
                    return BadRequest(validationResponse);
                }

                if (estadoActualizarDto == null)
                {
                    var badRequestResponse = ApiResponse<object>.ErrorResponse(
                        "Datos inválidos",
                        "Los datos del estado son requeridos"
                    );
                    badRequestResponse.Path = Request.Path;
                    return BadRequest(badRequestResponse);
                }

                if (idEstado != estadoActualizarDto.IdEstado)
                {
                    var badRequestResponse = ApiResponse<object>.ErrorResponse(
                        "ID inconsistente",
                        "El ID en la URL no coincide con el ID en los datos"
                    );
                    badRequestResponse.Path = Request.Path;
                    return BadRequest(badRequestResponse);
                }

                // Verificar si el estado existe
                if (!_estadoRepo.ExisteEstado(idEstado))
                {
                    var notFoundResponse = ApiResponse<object>.ErrorResponse(
                        "Estado no encontrado",
                        $"No se encontró un estado con el ID {idEstado}"
                    );
                    notFoundResponse.Path = Request.Path;
                    return NotFound(notFoundResponse);
                }

                // Verificar conflictos de nombre (excluyendo el estado actual)
                if (_estadoRepo.ExisteEstadoExcluyendoId(estadoActualizarDto.NombreEstado, idEstado))
                {
                    var conflictResponse = ApiResponse<object>.ErrorResponse(
                        "Conflicto de datos",
                        $"Ya existe otro estado con el nombre '{estadoActualizarDto.NombreEstado}'"
                    );
                    conflictResponse.Path = Request.Path;
                    return Conflict(conflictResponse);
                }

                // Verificar conflictos de código (excluyendo el estado actual)
                if (_estadoRepo.ExisteCodigoEstadoExcluyendoId(estadoActualizarDto.CodigoEstado, idEstado))
                {
                    var conflictResponse = ApiResponse<object>.ErrorResponse(
                        "Conflicto de datos",
                        $"Ya existe otro estado con el código '{estadoActualizarDto.CodigoEstado}'"
                    );
                    conflictResponse.Path = Request.Path;
                    return Conflict(conflictResponse);
                }

                var estado = _mapper.Map<Estado>(estadoActualizarDto);

                if (!_estadoRepo.ActualizarEstado(estado))
                {
                    var errorResponse = ApiResponse<object>.ErrorResponse(
                        "Error al actualizar estado",
                        "No se pudo actualizar el estado en la base de datos"
                    );
                    errorResponse.Path = Request.Path;
                    return StatusCode(500, errorResponse);
                }

                // Obtener el estado actualizado
                var estadoActualizado = _estadoRepo.GetEstado(idEstado);
                var estadoDto = _mapper.Map<EstadoDto>(estadoActualizado);

                var response = ApiResponse<EstadoDto>.SuccessResponse(
                    estadoDto,
                    "Estado actualizado exitosamente"
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

        /// <summary>
        /// Cambiar el estado de un registro de estado (activar/desactivar)
        /// </summary>
        /// <param name="idEstado">ID del estado</param>
        /// <param name="activar">true para activar, false para desactivar</param>
        /// <returns>Confirmación del cambio de estado</returns>
        [HttpPatch("{idEstado:int}/estado")]
        [ProducesResponseType(typeof(ApiResponse<EstadoDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.InternalServerError)]
        public IActionResult CambiarEstadoRegistro(int idEstado, [FromQuery] bool activar = true)
        {
            try
            {
                if (idEstado <= 0)
                {
                    var badRequestResponse = ApiResponse<object>.ErrorResponse(
                        "ID inválido",
                        "El ID del estado debe ser mayor a 0"
                    );
                    badRequestResponse.Path = Request.Path;
                    return BadRequest(badRequestResponse);
                }

                var estado = _estadoRepo.GetEstado(idEstado);
                if (estado == null)
                {
                    var notFoundResponse = ApiResponse<object>.ErrorResponse(
                        "Estado no encontrado",
                        $"No se encontró un estado con el ID {idEstado}"
                    );
                    notFoundResponse.Path = Request.Path;
                    return NotFound(notFoundResponse);
                }

                estado.EstadoRegistro = activar;

                if (!_estadoRepo.ActualizarEstado(estado))
                {
                    var errorResponse = ApiResponse<object>.ErrorResponse(
                        "Error al cambiar estado",
                        "No se pudo cambiar el estado del registro"
                    );
                    errorResponse.Path = Request.Path;
                    return StatusCode(500, errorResponse);
                }

                var estadoDto = _mapper.Map<EstadoDto>(estado);

                var response = ApiResponse<EstadoDto>.SuccessResponse(
                    estadoDto,
                    $"Estado {(activar ? "activado" : "desactivado")} exitosamente"
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
