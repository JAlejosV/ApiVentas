using ApiVentas.Attributes;
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
    [Route("api/v1/almacenes")]
    [Authorize]
    [ApiController]    
    public class AlmacenesController : ControllerBase
    {
        private readonly IAlmacenRepositorio _almacenRepo;
        private readonly IMapper _mapper;

        public AlmacenesController(IAlmacenRepositorio almacenRepo, IMapper mapper)
        {
            _almacenRepo = almacenRepo;
            _mapper = mapper;
        }

        /// <summary>
        /// Obtener todos los almacenes con paginación
        /// </summary>
        /// <param name="page">Número de página (por defecto 1)</param>
        /// <param name="pageSize">Tamaño de página (por defecto 10, máximo 100)</param>
        /// <param name="search">Término de búsqueda opcional</param>
        /// <param name="estadoRegistro">Filtro por estado: true = solo activos, false = solo inactivos, null = todos (por defecto)</param>
        /// <returns>Lista paginada de almacenes</returns>
        //[AllowAnonymous]
        [HttpGet]
        [PermissionRequired("Almacenes", "VerListado")]
        [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<AlmacenDto>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.InternalServerError)]
        public IActionResult GetAlmacenes(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string search = null,
            [FromQuery] bool? estadoRegistro = null)
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
                var listaAlmacenes = _almacenRepo.GetAlmacenes();

                // Aplicar filtro por estado
                if (estadoRegistro.HasValue)
                {
                    listaAlmacenes = listaAlmacenes.Where(a => a.EstadoRegistro == estadoRegistro.Value).ToList();
                }
                // Si es null, devuelve todos (activos e inactivos)

                if (!string.IsNullOrWhiteSpace(search))
                {
                    listaAlmacenes = listaAlmacenes.Where(a => 
                        a.NombreAlmacen.ToLower().Contains(search.ToLower()) ||
                        a.CodigoAlmacen.ToString().Contains(search)
                    ).ToList();
                }

                // Calcular totales
                var totalRecords = listaAlmacenes.Count();

                // Aplicar paginación
                var almacenesPaginados = listaAlmacenes
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                // Mapear a DTOs
                var almacenesDto = almacenesPaginados.Select(a => _mapper.Map<AlmacenDto>(a)).ToList();

                // Crear respuesta paginada
                var paginatedData = new PaginatedResponse<AlmacenDto>(almacenesDto, page, pageSize, totalRecords);

                var response = ApiResponse<PaginatedResponse<AlmacenDto>>.SuccessResponse(
                    paginatedData,
                    "Almacenes obtenidos exitosamente"
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
        /// Obtener un almacén específico por ID
        /// </summary>
        /// <param name="idAlmacen">ID del almacén</param>
        /// <returns>Información detallada del almacén</returns>
        [HttpGet("{idAlmacen:int}", Name = "GetAlmacen")]
        [PermissionRequired("Almacenes", "VerDetalle")]
        [ProducesResponseType(typeof(ApiResponse<AlmacenDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.InternalServerError)]
        public IActionResult GetAlmacen(int idAlmacen)
        {
            try
            {
                if (idAlmacen <= 0)
                {
                    var badRequestResponse = ApiResponse<object>.ErrorResponse(
                        "ID inválido",
                        "El ID del almacén debe ser mayor a 0"
                    );
                    badRequestResponse.Path = Request.Path;
                    return BadRequest(badRequestResponse);
                }

                var almacen = _almacenRepo.GetAlmacen(idAlmacen);

                if (almacen == null)
                {
                    var notFoundResponse = ApiResponse<object>.ErrorResponse(
                        "Almacén no encontrado",
                        $"No se encontró un almacén con el ID {idAlmacen}"
                    );
                    notFoundResponse.Path = Request.Path;
                    return NotFound(notFoundResponse);
                }

                var almacenDto = _mapper.Map<AlmacenDto>(almacen);

                var response = ApiResponse<AlmacenDto>.SuccessResponse(
                    almacenDto,
                    "Almacén obtenido exitosamente"
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
        /// Crear un nuevo almacén
        /// </summary>
        /// <param name="almacenCrearDto">Datos del almacén a crear</param>
        /// <returns>Almacén creado</returns>        
        [HttpPost]
        [PermissionRequired("Almacenes", "Crear")]
        [ProducesResponseType(typeof(ApiResponse<AlmacenDto>), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.InternalServerError)]
        public IActionResult CrearAlmacen([FromBody] AlmacenCrearDto almacenCrearDto)
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

                if (almacenCrearDto == null)
                {
                    var badRequestResponse = ApiResponse<object>.ErrorResponse(
                        "Datos inválidos",
                        "Los datos del almacén son requeridos"
                    );
                    badRequestResponse.Path = Request.Path;
                    return BadRequest(badRequestResponse);
                }

                // Verificar si ya existe un almacén con el mismo nombre
                if (_almacenRepo.ExisteAlmacen(almacenCrearDto.NombreAlmacen))
                {
                    var conflictResponse = ApiResponse<object>.ErrorResponse(
                        "Conflicto de datos",
                        $"Ya existe un almacén con el nombre '{almacenCrearDto.NombreAlmacen}'"
                    );
                    conflictResponse.Path = Request.Path;
                    return Conflict(conflictResponse);
                }

                // Verificar si ya existe un almacén con el mismo código
                if (_almacenRepo.ExisteCodigoAlmacen(almacenCrearDto.CodigoAlmacen))
                {
                    var conflictResponse = ApiResponse<object>.ErrorResponse(
                        "Conflicto de datos",
                        $"Ya existe un almacén con el código '{almacenCrearDto.CodigoAlmacen}'"
                    );
                    conflictResponse.Path = Request.Path;
                    return Conflict(conflictResponse);
                }

                var almacen = _mapper.Map<Almacen>(almacenCrearDto);

                if (!_almacenRepo.CrearAlmacen(almacen))
                {
                    var errorResponse = ApiResponse<object>.ErrorResponse(
                        "Error al crear almacén",
                        "No se pudo crear el almacén en la base de datos"
                    );
                    errorResponse.Path = Request.Path;
                    return StatusCode(500, errorResponse);
                }

                var almacenDto = _mapper.Map<AlmacenDto>(almacen);

                var response = ApiResponse<AlmacenDto>.SuccessResponse(
                    almacenDto,
                    "Almacén creado exitosamente"
                );
                response.Path = Request.Path;

                return CreatedAtRoute("GetAlmacen", new { idAlmacen = almacen.IdAlmacen }, response);
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
        /// Actualizar un almacén existente
        /// </summary>
        /// <param name="idAlmacen">ID del almacén a actualizar</param>
        /// <param name="almacenActualizarDto">Datos actualizados del almacén</param>
        /// <returns>Almacén actualizado</returns>
        [HttpPut("{idAlmacen:int}", Name = "ActualizarAlmacen")]
        [PermissionRequired("Almacenes", "Editar")]
        [ProducesResponseType(typeof(ApiResponse<AlmacenDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.InternalServerError)]
        public IActionResult ActualizarAlmacen(int idAlmacen, [FromBody] AlmacenActualizarDto almacenActualizarDto)
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

                if (almacenActualizarDto == null)
                {
                    var badRequestResponse = ApiResponse<object>.ErrorResponse(
                        "Datos inválidos",
                        "Los datos del almacén son requeridos"
                    );
                    badRequestResponse.Path = Request.Path;
                    return BadRequest(badRequestResponse);
                }

                if (idAlmacen != almacenActualizarDto.IdAlmacen)
                {
                    var badRequestResponse = ApiResponse<object>.ErrorResponse(
                        "ID inconsistente",
                        "El ID en la URL no coincide con el ID en los datos"
                    );
                    badRequestResponse.Path = Request.Path;
                    return BadRequest(badRequestResponse);
                }

                // Verificar si el almacén existe
                if (!_almacenRepo.ExisteAlmacen(idAlmacen))
                {
                    var notFoundResponse = ApiResponse<object>.ErrorResponse(
                        "Almacén no encontrado",
                        $"No se encontró un almacén con el ID {idAlmacen}"
                    );
                    notFoundResponse.Path = Request.Path;
                    return NotFound(notFoundResponse);
                }

                // Verificar conflictos de nombre (excluyendo el almacén actual)
                if (_almacenRepo.ExisteAlmacenExcluyendoId(almacenActualizarDto.NombreAlmacen, idAlmacen))
                {
                    var conflictResponse = ApiResponse<object>.ErrorResponse(
                        "Conflicto de datos",
                        $"Ya existe otro almacén con el nombre '{almacenActualizarDto.NombreAlmacen}'"
                    );
                    conflictResponse.Path = Request.Path;
                    return Conflict(conflictResponse);
                }

                // Verificar conflictos de código (excluyendo el almacén actual)
                if (_almacenRepo.ExisteCodigoAlmacenExcluyendoId(almacenActualizarDto.CodigoAlmacen, idAlmacen))
                {
                    var conflictResponse = ApiResponse<object>.ErrorResponse(
                        "Conflicto de datos",
                        $"Ya existe otro almacén con el código '{almacenActualizarDto.CodigoAlmacen}'"
                    );
                    conflictResponse.Path = Request.Path;
                    return Conflict(conflictResponse);
                }

                var almacen = _mapper.Map<Almacen>(almacenActualizarDto);

                if (!_almacenRepo.ActualizarAlmacen(almacen))
                {
                    var errorResponse = ApiResponse<object>.ErrorResponse(
                        "Error al actualizar almacén",
                        "No se pudo actualizar el almacén en la base de datos"
                    );
                    errorResponse.Path = Request.Path;
                    return StatusCode(500, errorResponse);
                }

                // Obtener el almacén actualizado
                var almacenActualizado = _almacenRepo.GetAlmacen(idAlmacen);
                var almacenDto = _mapper.Map<AlmacenDto>(almacenActualizado);

                var response = ApiResponse<AlmacenDto>.SuccessResponse(
                    almacenDto,
                    "Almacén actualizado exitosamente"
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
        /// Cambiar el estado de un almacén (activar/desactivar)
        /// </summary>
        /// <param name="idAlmacen">ID del almacén</param>
        /// <param name="activar">true para activar, false para desactivar</param>
        /// <returns>Confirmación del cambio de estado</returns>
        [HttpPatch("{idAlmacen:int}/estado")]
        [PermissionRequired("Almacenes", "Anular-Habilitar")]
        [ProducesResponseType(typeof(ApiResponse<AlmacenDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.InternalServerError)]
        public IActionResult CambiarEstadoAlmacen(int idAlmacen, [FromQuery] bool activar = true)
        {
            try
            {
                if (idAlmacen <= 0)
                {
                    var badRequestResponse = ApiResponse<object>.ErrorResponse(
                        "ID inválido",
                        "El ID del almacén debe ser mayor a 0"
                    );
                    badRequestResponse.Path = Request.Path;
                    return BadRequest(badRequestResponse);
                }

                var almacen = _almacenRepo.GetAlmacen(idAlmacen);
                if (almacen == null)
                {
                    var notFoundResponse = ApiResponse<object>.ErrorResponse(
                        "Almacén no encontrado",
                        $"No se encontró un almacén con el ID {idAlmacen}"
                    );
                    notFoundResponse.Path = Request.Path;
                    return NotFound(notFoundResponse);
                }

                almacen.EstadoRegistro = activar;

                if (!_almacenRepo.ActualizarAlmacen(almacen))
                {
                    var errorResponse = ApiResponse<object>.ErrorResponse(
                        "Error al cambiar estado",
                        "No se pudo cambiar el estado del almacén"
                    );
                    errorResponse.Path = Request.Path;
                    return StatusCode(500, errorResponse);
                }

                var almacenDto = _mapper.Map<AlmacenDto>(almacen);

                var response = ApiResponse<AlmacenDto>.SuccessResponse(
                    almacenDto,
                    $"Almacén {(activar ? "activado" : "desactivado")} exitosamente"
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
