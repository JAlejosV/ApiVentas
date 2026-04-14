using ApiVentas.Attributes;
using ApiVentas.Modelos;
using ApiVentas.Modelos.ApiResponse;
using ApiVentas.Modelos.Dtos;
using ApiVentas.Repositorio.IRepositorio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace ApiVentas.Controllers
{
    /// <summary>
    /// Controlador para la gestión de productos y sus relaciones con proveedores
    /// </summary>
    [Route("api/v1/productos")]
    [ApiController]
    [Authorize]
    public class ProductosController : ControllerBase
    {
        private readonly IProductoRepositorio _productoRepo;
        private readonly IProveedorRepositorio _proveedorRepo;

        public ProductosController(
            IProductoRepositorio productoRepo, 
            IProveedorRepositorio proveedorRepo)
        {
            _productoRepo = productoRepo;
            _proveedorRepo = proveedorRepo;
        }

        /// <summary>
        /// Obtener lista paginada de productos con información de proveedores
        /// </summary>
        /// <param name="page">Número de página (por defecto: 1)</param>
        /// <param name="pageSize">Tamaño de página (por defecto: 10, máximo: 100)</param>
        /// <param name="search">Término de búsqueda por código o nombre</param>
        /// <param name="estadoRegistro">Filtro por estado: true = solo activos, false = solo inactivos, null = todos (por defecto)</param>
        /// <returns>Lista paginada de productos</returns>
        [HttpGet]
        [PermissionRequired("Productos", "VerListado")]
        [ProducesResponseType(typeof(PaginatedResponse<ProductoDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetProductos(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string search = null,
            [FromQuery] bool? estadoRegistro = null)
        {
            try
            {
                // Validar parámetros de paginación
                if (page < 1)
                {
                    var badRequestResponse = ApiResponse<object>.ErrorResponse(
                        "Número de página inválido",
                        "El número de página debe ser mayor a 0"
                    );
                    badRequestResponse.Path = Request.Path;
                    return BadRequest(badRequestResponse);
                }

                if (pageSize < 1 || pageSize > 100)
                {
                    var badRequestResponse = ApiResponse<object>.ErrorResponse(
                        "Tamaño de página inválido",
                        "El tamaño de página debe estar entre 1 y 100"
                    );
                    badRequestResponse.Path = Request.Path;
                    return BadRequest(badRequestResponse);
                }

                var productos = await _productoRepo.GetProductos();
                
                // Aplicar filtros
                var productosFilteredQuery = productos.AsQueryable();
                
                // Aplicar filtro por estado
                if (estadoRegistro.HasValue)
                {
                    productosFilteredQuery = productosFilteredQuery.Where(p => p.EstadoRegistro == estadoRegistro.Value);
                }
                // Si es null, devuelve todos (activos e inactivos)

                if (!string.IsNullOrEmpty(search))
                {
                    productosFilteredQuery = productosFilteredQuery.Where(p => 
                        p.CodigoProducto.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        p.NombreProducto.Contains(search, StringComparison.OrdinalIgnoreCase));
                }

                var totalRecords = productosFilteredQuery.Count();

                // Aplicar paginación
                var productosPaginados = productosFilteredQuery
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var response = PaginatedResponse<ProductoDto>.CreateSuccessResponse(
                    productosPaginados,
                    page,
                    pageSize,
                    totalRecords,
                    "Productos obtenidos exitosamente"
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
        /// Obtener un producto específico por ID con información detallada de proveedores
        /// </summary>
        /// <param name="id">ID del producto</param>
        /// <returns>Información detallada del producto</returns>
        [HttpGet("{id:int}", Name = "GetProducto")]
        [PermissionRequired("Productos", "VerDetalle")]
        [ProducesResponseType(typeof(ApiResponse<ProductoDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetProducto(int id)
        {
            try
            {
                if (id <= 0)
                {
                    var badRequestResponse = ApiResponse<object>.ErrorResponse(
                        "ID inválido",
                        "El ID del producto debe ser mayor a 0"
                    );
                    badRequestResponse.Path = Request.Path;
                    return BadRequest(badRequestResponse);
                }

                var producto = await _productoRepo.GetProducto(id);
                if (producto == null)
                {
                    var notFoundResponse = ApiResponse<object>.ErrorResponse(
                        "Producto no encontrado",
                        $"No se encontró un producto con el ID {id}"
                    );
                    notFoundResponse.Path = Request.Path;
                    return NotFound(notFoundResponse);
                }

                var response = ApiResponse<ProductoDto>.SuccessResponse(
                    producto,
                    "Producto obtenido exitosamente"
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
        /// Crear un nuevo producto con sus relaciones de proveedores
        /// </summary>
        /// <param name="productoCrearDto">Datos del producto a crear</param>
        /// <returns>Confirmación de creación</returns>
        [HttpPost]
        [PermissionRequired("Productos", "Crear")]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CrearProducto([FromBody] ProductoCrearDto productoCrearDto)
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

                // Verificar si el código de producto ya existe
                if (await _productoRepo.ExisteProductoPorCodigo(productoCrearDto.CodigoProducto))
                {
                    var conflictResponse = ApiResponse<object>.ErrorResponse(
                        "Código de producto duplicado",
                        $"Ya existe un producto con el código {productoCrearDto.CodigoProducto}"
                    );
                    conflictResponse.Path = Request.Path;
                    return Conflict(conflictResponse);
                }

                // Validar que todos los proveedores existan
                foreach (var proveedorDto in productoCrearDto.Proveedores)
                {
                    if (!await _proveedorRepo.ExisteProveedorAsync(proveedorDto.IdProveedor))
                    {
                        var badRequestResponse = ApiResponse<object>.ErrorResponse(
                            "Proveedor inválido",
                            $"El proveedor con ID {proveedorDto.IdProveedor} no existe"
                        );
                        badRequestResponse.Path = Request.Path;
                        return BadRequest(badRequestResponse);
                    }
                }

                var idProductoCreado = await _productoRepo.CrearProducto(productoCrearDto);
                if (!idProductoCreado.HasValue)
                {
                    var errorResponse = ApiResponse<object>.ErrorResponse(
                        "Error al crear producto",
                        "No se pudo crear el producto en la base de datos"
                    );
                    errorResponse.Path = Request.Path;
                    return StatusCode(500, errorResponse);
                }

                // Obtener el producto recién creado para devolverlo en la respuesta
                var productoCreado = await _productoRepo.GetProducto(idProductoCreado.Value);

                var response = ApiResponse<ProductoDto>.SuccessResponse(
                    productoCreado,
                    "Producto creado exitosamente"
                );
                response.Path = Request.Path;

                return StatusCode(201, response);
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
        /// Actualizar un producto existente
        /// </summary>
        /// <param name="id">ID del producto</param>
        /// <param name="productoActualizarDto">Datos actualizados del producto</param>
        /// <returns>Confirmación de actualización</returns>
        //[HttpPatch("{id:int}")]
        [HttpPut("{id:int}")]
        [PermissionRequired("Productos", "Editar")]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ActualizarProducto(int id, [FromBody] ProductoActualizarDto productoActualizarDto)
        {
            try
            {
                if (id <= 0)
                {
                    var badRequestResponse = ApiResponse<object>.ErrorResponse(
                        "ID inválido",
                        "El ID del producto debe ser mayor a 0"
                    );
                    badRequestResponse.Path = Request.Path;
                    return BadRequest(badRequestResponse);
                }

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

                if (productoActualizarDto.IdProducto != id)
                {
                    var badRequestResponse = ApiResponse<object>.ErrorResponse(
                        "ID inconsistente",
                        "El ID del producto en la URL no coincide con el ID en los datos"
                    );
                    badRequestResponse.Path = Request.Path;
                    return BadRequest(badRequestResponse);
                }

                // Verificar si el producto existe
                if (!await _productoRepo.ExisteProducto(id))
                {
                    var notFoundResponse = ApiResponse<object>.ErrorResponse(
                        "Producto no encontrado",
                        $"No se encontró un producto con el ID {id}"
                    );
                    notFoundResponse.Path = Request.Path;
                    return NotFound(notFoundResponse);
                }

                // Validar que todos los proveedores activos existan
                foreach (var proveedorDto in productoActualizarDto.Proveedores.Where(p => p.EstadoRegistro))
                {
                    if (!await _proveedorRepo.ExisteProveedorAsync(proveedorDto.IdProveedor))
                    {
                        var badRequestResponse = ApiResponse<object>.ErrorResponse(
                            "Proveedor inválido",
                            $"El proveedor con ID {proveedorDto.IdProveedor} no existe"
                        );
                        badRequestResponse.Path = Request.Path;
                        return BadRequest(badRequestResponse);
                    }
                }

                var resultado = await _productoRepo.ActualizarProducto(productoActualizarDto);
                if (!resultado)
                {
                    var errorResponse = ApiResponse<object>.ErrorResponse(
                        "Error al actualizar producto",
                        "No se pudo actualizar el producto en la base de datos"
                    );
                    errorResponse.Path = Request.Path;
                    return StatusCode(500, errorResponse);
                }

                // Obtener el producto actualizado para devolverlo en la respuesta
                var productoActualizado = await _productoRepo.GetProducto(id);

                var response = ApiResponse<ProductoDto>.SuccessResponse(
                    productoActualizado,
                    "Producto actualizado exitosamente"
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
        /// Cambiar el estado de registro de un producto (habilitar/anular)
        /// </summary>
        /// <param name="id">ID del producto</param>
        /// <param name="activar">true para habilitar, false para anular</param>
        /// <returns>Confirmación del cambio de estado</returns>
        [HttpPatch("{id:int}/estado")]
        [PermissionRequired("Productos", "Anular-Habilitar")]
        [ProducesResponseType(typeof(ApiResponse<ProductoDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CambiarEstadoRegistro(int id, [FromQuery] bool activar = true)
        {
            try
            {
                if (id <= 0)
                {
                    var badRequestResponse = ApiResponse<object>.ErrorResponse(
                        "ID inválido",
                        "El ID del producto debe ser mayor a 0"
                    );
                    badRequestResponse.Path = Request.Path;
                    return BadRequest(badRequestResponse);
                }

                if (!await _productoRepo.ExisteProducto(id))
                {
                    var notFoundResponse = ApiResponse<object>.ErrorResponse(
                        "Producto no encontrado",
                        $"No se encontró un producto con el ID {id}"
                    );
                    notFoundResponse.Path = Request.Path;
                    return NotFound(notFoundResponse);
                }

                bool resultado = activar 
                    ? await _productoRepo.HabilitarProducto(id)
                    : await _productoRepo.AnularProducto(id);

                string accion = activar ? "habilitado" : "anulado";

                if (!resultado)
                {
                    var errorResponse = ApiResponse<object>.ErrorResponse(
                        $"Error al {accion.Replace("ado", "ar")} producto",
                        $"No se pudo {accion.Replace("ado", "ar")} el producto"
                    );
                    errorResponse.Path = Request.Path;
                    return StatusCode(500, errorResponse);
                }

                // Obtener el producto actualizado para devolverlo en la respuesta
                var productoActualizado = await _productoRepo.GetProducto(id);

                var response = ApiResponse<ProductoDto>.SuccessResponse(
                    productoActualizado,
                    $"Producto {accion} exitosamente"
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
