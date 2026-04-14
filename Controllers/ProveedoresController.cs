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
using System.Security.Cryptography;
using System.Text;

namespace ApiVentas.Controllers
{
    [Route("api/v1/proveedores")]
    [Authorize]
    [ApiController]
    public class ProveedoresController : ControllerBase
    {
        private readonly IProveedorRepositorio _proveedorRepo;
        private readonly IProveedorArchivoRepositorio _archivoRepo;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProveedoresController(IProveedorRepositorio proveedorRepo, IProveedorArchivoRepositorio archivoRepo, IMapper mapper, IWebHostEnvironment webHostEnvironment)
        {
            _proveedorRepo = proveedorRepo;
            _archivoRepo = archivoRepo;
            _mapper = mapper;
            _webHostEnvironment = webHostEnvironment;
        }

        /// <summary>
        /// Obtener todos los proveedores con paginación
        /// </summary>
        /// <param name="page">Número de página (por defecto 1)</param>
        /// <param name="pageSize">Tamaño de página (por defecto 10, máximo 100)</param>
        /// <param name="search">Término de búsqueda opcional</param>
        /// <param name="estadoRegistro">Filtro por estado: true = solo activos, false = solo inactivos, null = todos (por defecto)</param>
        /// <returns>Lista paginada de proveedores</returns>
        [HttpGet]
        [PermissionRequired("Proveedores", "VerListado")]
        [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<ProveedorDto>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.InternalServerError)]
        public IActionResult GetProveedores(
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
                var listaProveedores = _proveedorRepo.GetProveedores();

                // Aplicar filtro por estado
                if (estadoRegistro.HasValue)
                {
                    listaProveedores = listaProveedores.Where(p => p.EstadoRegistro == estadoRegistro.Value).ToList();
                }
                // Si es null, devuelve todos (activos e inactivos)

                if (!string.IsNullOrWhiteSpace(search))
                {
                    listaProveedores = listaProveedores.Where(p => 
                        p.NombreProveedor.ToLower().Contains(search.ToLower()) ||
                        p.CodigoProveedor.ToString().Contains(search) ||
                        (!string.IsNullOrEmpty(p.Telefono) && p.Telefono.ToLower().Contains(search.ToLower())) ||
                        (!string.IsNullOrEmpty(p.Contacto) && p.Contacto.ToLower().Contains(search.ToLower()))
                    ).ToList();
                }

                // Calcular totales
                var totalRecords = listaProveedores.Count();

                // Aplicar paginación
                var proveedoresPaginados = listaProveedores
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                // Mapear a DTOs
                var proveedoresDto = proveedoresPaginados.Select(p => _mapper.Map<ProveedorDto>(p)).ToList();

                // Crear respuesta paginada
                var paginatedData = new PaginatedResponse<ProveedorDto>(proveedoresDto, page, pageSize, totalRecords);

                var response = ApiResponse<PaginatedResponse<ProveedorDto>>.SuccessResponse(
                    paginatedData,
                    "Proveedores obtenidos exitosamente"
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
        /// Obtener un proveedor específico por ID
        /// </summary>
        /// <param name="idProveedor">ID del proveedor</param>
        /// <returns>Información detallada del proveedor</returns>
        [HttpGet("{idProveedor:int}", Name = "GetProveedor")]
        [PermissionRequired("Proveedores", "VerDetalle")]
        [ProducesResponseType(typeof(ApiResponse<ProveedorDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.InternalServerError)]
        public IActionResult GetProveedor(int idProveedor)
        {
            try
            {
                if (idProveedor <= 0)
                {
                    var badRequestResponse = ApiResponse<object>.ErrorResponse(
                        "ID inválido",
                        "El ID del proveedor debe ser mayor a 0"
                    );
                    badRequestResponse.Path = Request.Path;
                    return BadRequest(badRequestResponse);
                }

                var proveedor = _proveedorRepo.GetProveedor(idProveedor);

                if (proveedor == null)
                {
                    var notFoundResponse = ApiResponse<object>.ErrorResponse(
                        "Proveedor no encontrado",
                        $"No se encontró un proveedor con el ID {idProveedor}"
                    );
                    notFoundResponse.Path = Request.Path;
                    return NotFound(notFoundResponse);
                }

                var proveedorDto = _mapper.Map<ProveedorDto>(proveedor);

                var response = ApiResponse<ProveedorDto>.SuccessResponse(
                    proveedorDto,
                    "Proveedor obtenido exitosamente"
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
        /// Crear un nuevo proveedor
        /// </summary>
        /// <param name="proveedorCrearDto">Datos del proveedor a crear</param>
        /// <returns>Proveedor creado</returns>
        [HttpPost]
        [PermissionRequired("Proveedores", "Crear")]
        [ProducesResponseType(typeof(ApiResponse<ProveedorDto>), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.InternalServerError)]
        public IActionResult CrearProveedor([FromBody] ProveedorCrearDto proveedorCrearDto)
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

                if (proveedorCrearDto == null)
                {
                    var badRequestResponse = ApiResponse<object>.ErrorResponse(
                        "Datos inválidos",
                        "Los datos del proveedor son requeridos"
                    );
                    badRequestResponse.Path = Request.Path;
                    return BadRequest(badRequestResponse);
                }

                // Verificar si ya existe un proveedor con el mismo nombre
                if (_proveedorRepo.ExisteProveedor(proveedorCrearDto.NombreProveedor))
                {
                    var conflictResponse = ApiResponse<object>.ErrorResponse(
                        "Conflicto de datos",
                        $"Ya existe un proveedor con el nombre '{proveedorCrearDto.NombreProveedor}'"
                    );
                    conflictResponse.Path = Request.Path;
                    return Conflict(conflictResponse);
                }

                // Verificar si ya existe un proveedor con el mismo código
                if (_proveedorRepo.ExisteCodigoProveedor(proveedorCrearDto.CodigoProveedor))
                {
                    var conflictResponse = ApiResponse<object>.ErrorResponse(
                        "Conflicto de datos",
                        $"Ya existe un proveedor con el código {proveedorCrearDto.CodigoProveedor}"
                    );
                    conflictResponse.Path = Request.Path;
                    return Conflict(conflictResponse);
                }

                var proveedor = _mapper.Map<Proveedor>(proveedorCrearDto);

                if (!_proveedorRepo.CrearProveedor(proveedor))
                {
                    var errorResponse = ApiResponse<object>.ErrorResponse(
                        "Error al crear proveedor",
                        "No se pudo crear el proveedor en la base de datos"
                    );
                    errorResponse.Path = Request.Path;
                    return StatusCode(500, errorResponse);
                }

                var proveedorDto = _mapper.Map<ProveedorDto>(proveedor);

                var response = ApiResponse<ProveedorDto>.SuccessResponse(
                    proveedorDto,
                    "Proveedor creado exitosamente"
                );
                response.Path = Request.Path;

                return CreatedAtRoute("GetProveedor", new { idProveedor = proveedor.IdProveedor }, response);
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
        /// Actualizar un proveedor existente
        /// </summary>
        /// <param name="idProveedor">ID del proveedor a actualizar</param>
        /// <param name="proveedorActualizarDto">Datos actualizados del proveedor</param>
        /// <returns>Proveedor actualizado</returns>
        //[HttpPatch("{idProveedor:int}", Name = "ActualizarProveedor")]
        [HttpPut("{idProveedor:int}", Name = "ActualizarProveedor")]
        [PermissionRequired("Proveedores", "Editar")]
        [ProducesResponseType(typeof(ApiResponse<ProveedorDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.InternalServerError)]
        public IActionResult ActualizarProveedor(int idProveedor, [FromBody] ProveedorActualizarDto proveedorActualizarDto)
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

                if (proveedorActualizarDto == null)
                {
                    var badRequestResponse = ApiResponse<object>.ErrorResponse(
                        "Datos inválidos",
                        "Los datos del proveedor son requeridos"
                    );
                    badRequestResponse.Path = Request.Path;
                    return BadRequest(badRequestResponse);
                }

                if (idProveedor != proveedorActualizarDto.IdProveedor)
                {
                    var badRequestResponse = ApiResponse<object>.ErrorResponse(
                        "ID inconsistente",
                        "El ID en la URL no coincide con el ID en los datos"
                    );
                    badRequestResponse.Path = Request.Path;
                    return BadRequest(badRequestResponse);
                }

                // Verificar si el proveedor existe
                if (!_proveedorRepo.ExisteProveedor(idProveedor))
                {
                    var notFoundResponse = ApiResponse<object>.ErrorResponse(
                        "Proveedor no encontrado",
                        $"No se encontró un proveedor con el ID {idProveedor}"
                    );
                    notFoundResponse.Path = Request.Path;
                    return NotFound(notFoundResponse);
                }

                // Verificar conflictos de nombre (excluyendo el proveedor actual)
                if (_proveedorRepo.ExisteProveedorExcluyendoId(proveedorActualizarDto.NombreProveedor, idProveedor))
                {
                    var conflictResponse = ApiResponse<object>.ErrorResponse(
                        "Conflicto de datos",
                        $"Ya existe otro proveedor con el nombre '{proveedorActualizarDto.NombreProveedor}'"
                    );
                    conflictResponse.Path = Request.Path;
                    return Conflict(conflictResponse);
                }

                // Verificar conflictos de código (excluyendo el proveedor actual)
                if (_proveedorRepo.ExisteCodigoProveedorExcluyendoId(proveedorActualizarDto.CodigoProveedor, idProveedor))
                {
                    var conflictResponse = ApiResponse<object>.ErrorResponse(
                        "Conflicto de datos",
                        $"Ya existe otro proveedor con el código {proveedorActualizarDto.CodigoProveedor}"
                    );
                    conflictResponse.Path = Request.Path;
                    return Conflict(conflictResponse);
                }

                var proveedor = _mapper.Map<Proveedor>(proveedorActualizarDto);

                if (!_proveedorRepo.ActualizarProveedor(proveedor))
                {
                    var errorResponse = ApiResponse<object>.ErrorResponse(
                        "Error al actualizar proveedor",
                        "No se pudo actualizar el proveedor en la base de datos"
                    );
                    errorResponse.Path = Request.Path;
                    return StatusCode(500, errorResponse);
                }

                // Obtener el proveedor actualizado
                var proveedorActualizado = _proveedorRepo.GetProveedor(idProveedor);
                var proveedorDto = _mapper.Map<ProveedorDto>(proveedorActualizado);

                var response = ApiResponse<ProveedorDto>.SuccessResponse(
                    proveedorDto,
                    "Proveedor actualizado exitosamente"
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
        /// Cambiar el estado de un registro de proveedor (activar/desactivar)
        /// </summary>
        /// <param name="idProveedor">ID del proveedor</param>
        /// <param name="activar">true para activar, false para desactivar</param>
        /// <returns>Confirmación del cambio de estado</returns>
        [HttpPatch("{idProveedor:int}/estado")]
        [PermissionRequired("Proveedores", "Anular-Habilitar")]
        [ProducesResponseType(typeof(ApiResponse<ProveedorDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CambiarEstadoRegistro(int idProveedor, [FromQuery] bool activar = true)
        {
            try
            {
                if (idProveedor <= 0)
                {
                    var badRequestResponse = ApiResponse<object>.ErrorResponse(
                        "ID inválido",
                        "El ID del proveedor debe ser mayor a 0"
                    );
                    badRequestResponse.Path = Request.Path;
                    return BadRequest(badRequestResponse);
                }

                if (!_proveedorRepo.ExisteProveedor(idProveedor))
                {
                    var notFoundResponse = ApiResponse<object>.ErrorResponse(
                        "Proveedor no encontrado",
                        $"No se encontró un proveedor con el ID {idProveedor}"
                    );
                    notFoundResponse.Path = Request.Path;
                    return NotFound(notFoundResponse);
                }

                bool resultado = activar 
                    ? await _proveedorRepo.HabilitarProveedor(idProveedor)
                    : await _proveedorRepo.AnularProveedor(idProveedor);

                if (!resultado)
                {
                    var errorResponse = ApiResponse<object>.ErrorResponse(
                        "Error al cambiar estado",
                        "No se pudo cambiar el estado del proveedor"
                    );
                    errorResponse.Path = Request.Path;
                    return StatusCode(500, errorResponse);
                }

                // Obtener el proveedor actualizado
                var proveedor = _proveedorRepo.GetProveedor(idProveedor);
                var proveedorDto = _mapper.Map<ProveedorDto>(proveedor);

                var response = ApiResponse<ProveedorDto>.SuccessResponse(
                    proveedorDto,
                    $"Proveedor {(activar ? "activado" : "desactivado")} exitosamente"
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

        #region "Endpoints para manejo de archivos con almacenamiento local"

        /// <summary>
        /// Obtener un proveedor con sus archivos asociados
        /// </summary>
        /// <param name="idProveedor">ID del proveedor</param>
        /// <returns>Información del proveedor con lista de archivos</returns>
        [HttpGet("{idProveedor:int}/con-archivos", Name = "GetProveedorConArchivos")]
        [PermissionRequired("Proveedores", "VerDetalle")]
        [ProducesResponseType(typeof(ApiResponse<ProveedorCabeceraDetalleDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.InternalServerError)]
        public IActionResult GetProveedorConArchivos(int idProveedor)
        {
            try
            {
                if (idProveedor <= 0)
                {
                    var badRequestResponse = ApiResponse<object>.ErrorResponse(
                        "ID inválido",
                        "El ID del proveedor debe ser mayor a 0"
                    );
                    badRequestResponse.Path = Request.Path;
                    return BadRequest(badRequestResponse);
                }

                var proveedor = _proveedorRepo.GetProveedor(idProveedor);
                if (proveedor == null)
                {
                    var notFoundResponse = ApiResponse<object>.ErrorResponse(
                        "Proveedor no encontrado",
                        $"No se encontró un proveedor con el ID {idProveedor}"
                    );
                    notFoundResponse.Path = Request.Path;
                    return NotFound(notFoundResponse);
                }

                var archivos = _archivoRepo.GetArchivosByProveedor(idProveedor);
                
                var resultado = new ProveedorCabeceraDetalleDto
                {
                    IdProveedor = proveedor.IdProveedor,
                    CodigoProveedor = proveedor.CodigoProveedor,
                    NombreProveedor = proveedor.NombreProveedor,
                    Telefono = proveedor.Telefono,
                    Contacto = proveedor.Contacto,
                    EstadoRegistro = proveedor.EstadoRegistro,
                    ArchivosExistentes = archivos.Select(a => new ProveedorArchivoDetalleDto
                    {
                        IdProveedorArchivo = a.IdProveedorArchivo,
                        NombreArchivo = a.NombreArchivo,
                        RutaArchivo = a.RutaArchivo
                    }).ToList()
                };

                var response = ApiResponse<ProveedorCabeceraDetalleDto>.SuccessResponse(
                    resultado,
                    "Proveedor con archivos obtenido exitosamente"
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
        /// Subir un archivo de catálogo para un proveedor
        /// </summary>
        /// <param name="idProveedor">ID del proveedor</param>
        /// <param name="archivo">Archivo a subir</param>
        /// <returns>Información del archivo subido</returns>
        [HttpPost("{idProveedor:int}/subir-archivo")]
        [PermissionRequired("Proveedores", "Editar")]
        public async Task<IActionResult> SubirArchivo(int idProveedor, IFormFile archivo)
        {
            try
            {
                if (idProveedor <= 0)
                {
                    var badRequestResponse = ApiResponse<object>.ErrorResponse(
                        "ID inválido",
                        "El ID del proveedor debe ser mayor a 0"
                    );
                    badRequestResponse.Path = Request.Path;
                    return BadRequest(badRequestResponse);
                }

                if (!_proveedorRepo.ExisteProveedor(idProveedor))
                {
                    var notFoundResponse = ApiResponse<object>.ErrorResponse(
                        "Proveedor no encontrado",
                        $"No se encontró un proveedor con el ID {idProveedor}"
                    );
                    notFoundResponse.Path = Request.Path;
                    return NotFound(notFoundResponse);
                }

                if (archivo == null || archivo.Length == 0)
                {
                    var badRequestResponse = ApiResponse<object>.ErrorResponse(
                        "Archivo requerido",
                        "Debe proporcionar un archivo para subir"
                    );
                    badRequestResponse.Path = Request.Path;
                    return BadRequest(badRequestResponse);
                }

                // Validar extensión de archivo
                var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
                var extensionesPermitidas = new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".png", ".jpg", ".jpeg", ".gif", ".txt" };
                
                if (!extensionesPermitidas.Contains(extension))
                {
                    var badRequestResponse = ApiResponse<object>.ErrorResponse(
                        "Tipo de archivo no permitido",
                        $"Solo se permiten archivos con las siguientes extensiones: {string.Join(", ", extensionesPermitidas)}"
                    );
                    badRequestResponse.Path = Request.Path;
                    return BadRequest(badRequestResponse);
                }

                // Validar tamaño de archivo (15MB máximo)
                if (archivo.Length > 15 * 1024 * 1024)
                {
                    var badRequestResponse = ApiResponse<object>.ErrorResponse(
                        "Archivo muy grande",
                        "El archivo no debe superar los 15MB"
                    );
                    badRequestResponse.Path = Request.Path;
                    return BadRequest(badRequestResponse);
                }

                try
                {
                    // Crear directorio si no existe
                    var uploadsPath = Path.Combine(_webHostEnvironment.WebRootPath, "archivos", "proveedores");
                    if (!Directory.Exists(uploadsPath))
                    {
                        Directory.CreateDirectory(uploadsPath);
                    }

                    // Generar nombre único para el archivo
                    var nombreUnico = $"proveedor_{idProveedor}_{Guid.NewGuid()}{extension}";
                    var rutaCompleta = Path.Combine(uploadsPath, nombreUnico);
                    var rutaRelativa = $"archivos/proveedores/{nombreUnico}";

                    // Guardar archivo físicamente
                    using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                    {
                        await archivo.CopyToAsync(stream);
                    }

                    // Guardar referencia en base de datos
                    var proveedorArchivo = new ProveedorArchivo
                    {
                        IdProveedor = idProveedor,
                        NombreArchivo = archivo.FileName,
                        RutaArchivo = rutaRelativa
                    };

                    if (!_archivoRepo.CrearArchivo(proveedorArchivo))
                    {
                        // Si falla la BD, eliminar archivo físico
                        if (System.IO.File.Exists(rutaCompleta))
                        {
                            System.IO.File.Delete(rutaCompleta);
                        }

                        var errorResponse = ApiResponse<object>.ErrorResponse(
                            "Error guardando archivo",
                            "No se pudo guardar la referencia del archivo en la base de datos"
                        );
                        errorResponse.Path = Request.Path;
                        return StatusCode(500, errorResponse);
                    }

                    var response = ApiResponse<object>.SuccessResponse(
                        new { 
                            mensaje = "Archivo subido exitosamente",
                            idArchivo = proveedorArchivo.IdProveedorArchivo,
                            nombreArchivo = archivo.FileName,
                            rutaArchivo = rutaRelativa,
                            tamanoArchivo = archivo.Length,
                            tipoArchivo = archivo.ContentType
                        },
                        "Archivo subido exitosamente al almacenamiento local"
                    );
                    response.Path = Request.Path;

                    return Ok(response);
                }
                catch (Exception ex)
                {
                    var errorResponse = ApiResponse<object>.ErrorResponse(
                        "Error subiendo archivo",
                        $"Error al subir el archivo al almacenamiento local: {ex.Message}"
                    );
                    errorResponse.Path = Request.Path;
                    return StatusCode(500, errorResponse);
                }
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
        /// Eliminar un archivo de catálogo
        /// </summary>
        /// <param name="idArchivo">ID del archivo</param>
        /// <returns>Confirmación de eliminación</returns>
        [HttpDelete("archivo/{idArchivo:int}")]
        [PermissionRequired("Proveedores", "Editar")]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> EliminarArchivo(int idArchivo)
        {
            try
            {
                if (idArchivo <= 0)
                {
                    var badRequestResponse = ApiResponse<object>.ErrorResponse(
                        "ID inválido",
                        "El ID del archivo debe ser mayor a 0"
                    );
                    badRequestResponse.Path = Request.Path;
                    return BadRequest(badRequestResponse);
                }

                var archivo = _archivoRepo.GetArchivo(idArchivo);
                if (archivo == null)
                {
                    var notFoundResponse = ApiResponse<object>.ErrorResponse(
                        "Archivo no encontrado",
                        $"No se encontró un archivo con el ID {idArchivo}"
                    );
                    notFoundResponse.Path = Request.Path;
                    return NotFound(notFoundResponse);
                }

                try
                {
                    // Eliminar archivo físico del almacenamiento local
                    var rutaCompleta = Path.Combine(_webHostEnvironment.WebRootPath, archivo.RutaArchivo);
                    if (System.IO.File.Exists(rutaCompleta))
                    {
                        System.IO.File.Delete(rutaCompleta);
                    }

                    // Eliminar registro de la base de datos
                    if (!_archivoRepo.BorrarArchivo(archivo))
                    {
                        var errorResponse = ApiResponse<object>.ErrorResponse(
                            "Error eliminando archivo",
                            "No se pudo eliminar el archivo de la base de datos"
                        );
                        errorResponse.Path = Request.Path;
                        return StatusCode(500, errorResponse);
                    }

                    var response = ApiResponse<object>.SuccessResponse(
                        new { 
                            mensaje = "Archivo eliminado exitosamente",
                            archivoEliminado = archivo.NombreArchivo
                        },
                        "Archivo eliminado exitosamente del almacenamiento local y base de datos"
                    );
                    response.Path = Request.Path;

                    return Ok(response);
                }
                catch (Exception ex)
                {
                    var errorResponse = ApiResponse<object>.ErrorResponse(
                        "Error eliminando archivo",
                        $"Error al eliminar el archivo del almacenamiento local: {ex.Message}"
                    );
                    errorResponse.Path = Request.Path;
                    return StatusCode(500, errorResponse);
                }
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
        /// Generar URL de vista previa temporal para un archivo
        /// </summary>
        /// <param name="idArchivo">ID del archivo</param>
        /// <returns>URL temporal para visualizar el archivo</returns>
        [HttpGet("archivo/{idArchivo:int}/preview")]
        [PermissionRequired("Proveedores", "VerDetalle")]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.InternalServerError)]
        public IActionResult GenerarTokenPreview(int idArchivo)
        {
            try
            {
                if (idArchivo <= 0)
                {
                    var badRequestResponse = ApiResponse<object>.ErrorResponse(
                        "ID inválido",
                        "El ID del archivo debe ser mayor a 0"
                    );
                    badRequestResponse.Path = Request.Path;
                    return BadRequest(badRequestResponse);
                }

                var archivo = _archivoRepo.GetArchivo(idArchivo);
                if (archivo == null)
                {
                    var notFoundResponse = ApiResponse<object>.ErrorResponse(
                        "Archivo no encontrado",
                        $"No se encontró un archivo con el ID {idArchivo}"
                    );
                    notFoundResponse.Path = Request.Path;
                    return NotFound(notFoundResponse);
                }

                // Generar token temporal (válido por 30 minutos)
                var token = GenerarToken(idArchivo);
                var urlPreview = Url.Action("VisualizarArchivo", "Proveedores", new { token = token }, Request.Scheme);

                var response = ApiResponse<object>.SuccessResponse(
                    new { 
                        url = urlPreview, 
                        token = token,
                        nombreArchivo = archivo.NombreArchivo,
                        expiracion = DateTime.UtcNow.AddMinutes(30).ToString("yyyy-MM-dd HH:mm:ss UTC")
                    },
                    "URL de vista previa generada exitosamente"
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
        /// Visualizar archivo mediante token temporal
        /// </summary>
        /// <param name="token">Token temporal de acceso</param>
        /// <returns>Archivo para visualización</returns>
        [HttpGet("visualizar")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> VisualizarArchivo(string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    var badRequestResponse = ApiResponse<object>.ErrorResponse(
                        "Token requerido",
                        "Debe proporcionar un token válido para acceder al archivo"
                    );
                    badRequestResponse.Path = Request.Path;
                    return BadRequest(badRequestResponse);
                }

                var idArchivo = ValidarToken(token);
                if (idArchivo == 0)
                {
                    var unauthorizedResponse = ApiResponse<object>.ErrorResponse(
                        "Token inválido",
                        "El token proporcionado es inválido o ha expirado"
                    );
                    unauthorizedResponse.Path = Request.Path;
                    return Unauthorized(unauthorizedResponse);
                }

                var archivo = _archivoRepo.GetArchivo(idArchivo);
                if (archivo == null)
                {
                    var notFoundResponse = ApiResponse<object>.ErrorResponse(
                        "Archivo no encontrado",
                        "No se encontró el archivo solicitado"
                    );
                    notFoundResponse.Path = Request.Path;
                    return NotFound(notFoundResponse);
                }

                // Obtener archivo del almacenamiento local
                var rutaCompleta = Path.Combine(_webHostEnvironment.WebRootPath, archivo.RutaArchivo);
                if (!System.IO.File.Exists(rutaCompleta))
                {
                    var notFoundResponse = ApiResponse<object>.ErrorResponse(
                        "Archivo físico no encontrado",
                        "El archivo no existe en el almacenamiento local"
                    );
                    notFoundResponse.Path = Request.Path;
                    return NotFound(notFoundResponse);
                }

                var extension = Path.GetExtension(archivo.NombreArchivo).ToLowerInvariant();
                var contentType = GetContentType(extension);

                var fileBytes = await System.IO.File.ReadAllBytesAsync(rutaCompleta);
                
                return File(fileBytes, contentType, archivo.NombreArchivo);
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.ErrorResponse(
                    "Error interno del servidor",
                    $"Error procesando el archivo: {ex.Message}"
                );
                errorResponse.Path = Request.Path;
                return StatusCode(500, errorResponse);
            }
        }

        #endregion

        #region "Métodos auxiliares"

        private string GenerarToken(int idArchivo)
        {
            var expiracion = DateTimeOffset.UtcNow.AddMinutes(30).ToUnixTimeSeconds();
            var data = $"{idArchivo}:{expiracion}";
            
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes("clave-secreta-para-tokens")))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
                var signature = Convert.ToBase64String(hash);
                return Convert.ToBase64String(Encoding.UTF8.GetBytes($"{data}:{signature}"));
            }
        }

        private int ValidarToken(string token)
        {
            try
            {
                var tokenData = Encoding.UTF8.GetString(Convert.FromBase64String(token));
                var parts = tokenData.Split(':');
                
                if (parts.Length != 3)
                    return 0;

                var idArchivo = int.Parse(parts[0]);
                var expiracion = long.Parse(parts[1]);
                var signature = parts[2];

                // Verificar expiración
                if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() > expiracion)
                    return 0;

                // Verificar firma
                var data = $"{parts[0]}:{parts[1]}";
                using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes("clave-secreta-para-tokens")))
                {
                    var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
                    var expectedSignature = Convert.ToBase64String(hash);
                    
                    if (signature != expectedSignature)
                        return 0;
                }

                return idArchivo;
            }
            catch
            {
                return 0;
            }
        }

        private string GetContentType(string extension)
        {
            return extension switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".txt" => "text/plain",
                _ => "application/octet-stream"
            };
        }

        #endregion
    }
}
