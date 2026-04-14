using ApiVentas.Attributes;
using ApiVentas.Modelos;
using ApiVentas.Modelos.ApiResponse;
using ApiVentas.Modelos.Dtos;
using ApiVentas.Repositorio;
using ApiVentas.Repositorio.IRepositorio;
using ApiVentas.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ApiVentas.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class PedidosController : ControllerBase
    {
        private readonly IPedidoRepositorio _pedidoRepo;
        private readonly IProductoProveedorRepositorio _productoProveedorRepo;
        private readonly IFileStorageService _fileStorage;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PedidosController(IPedidoRepositorio pedidoRepo, IProductoProveedorRepositorio productoProveedorRepo, IFileStorageService fileStorage, IMapper mapper, IWebHostEnvironment webHostEnvironment)
        {
            _pedidoRepo = pedidoRepo;
            _productoProveedorRepo = productoProveedorRepo;
            _fileStorage = fileStorage;
            _mapper = mapper;
            _webHostEnvironment = webHostEnvironment;
        }

        /// <summary>
        /// Obtener todos los pedidos con paginación y filtros
        /// </summary>
        /// <param name="page">Número de página (por defecto 1)</param>
        /// <param name="pageSize">Tamaño de página (por defecto 10)</param>
        /// <param name="search">Texto de búsqueda (opcional)</param>
        /// <param name="idProveedor">Filtro por proveedor (opcional)</param>
        /// <param name="idEstado">Filtro por ID de estado: 1=Registrado, 2=Verificado, 3=Trasladado, 4=Anulado (opcional)</param>
        /// <param name="fechaInicio">Fecha inicio (por defecto 1 semana antes)</param>
        /// <param name="fechaFin">Fecha fin (por defecto fecha actual)</param>
        /// <returns></returns>
        [HttpGet]
        [PermissionRequired("Pedidos", "VerListado")]
        public async Task<IActionResult> GetPedidos(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string search = "",
            [FromQuery] int? idProveedor = null,
            [FromQuery] int? idEstado = null,
            [FromQuery] DateTime? fechaInicio = null,
            [FromQuery] DateTime? fechaFin = null)
        {
            try
            {
                // Configurar fechas por defecto solo si no se especifican
                var fechaInicioFiltro = fechaInicio ?? DateTime.Now.AddYears(-1); // Ampliar rango para incluir más registros
                var fechaFinFiltro = fechaFin ?? DateTime.Now.AddDays(1); // Incluir registros de hoy

                var filtro = new PedidoFiltroDto
                {
                    IdProveedor = idProveedor,
                    NombreProveedor = search,
                    IdEstado = idEstado,
                    FechaInicio = fechaInicioFiltro,
                    FechaFin = fechaFinFiltro
                };

                var pedidos = await _pedidoRepo.BuscarPedidos(filtro);
                
                // Si no hay pedidos con filtros, intentar obtener todos para debug
                if (pedidos.Count == 0)
                {
                    var todosPedidos = await _pedidoRepo.GetPedidos();
                    if (todosPedidos.Count > 0)
                    {
                        // Log para debug - hay pedidos pero los filtros los excluyen
                        Console.WriteLine($"[DEBUG] Encontrados {todosPedidos.Count} pedidos totales, pero 0 con los filtros aplicados.");
                        Console.WriteLine($"[DEBUG] Filtros: IdProveedor={idProveedor}, IdEstado={idEstado}, FechaInicio={fechaInicioFiltro}, FechaFin={fechaFinFiltro}");
                    }
                }
                
                var totalRegistros = pedidos.Count;
                var totalPaginas = (int)Math.Ceiling(totalRegistros / (double)pageSize);
                
                var pedidosPaginados = pedidos
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var response = PaginatedResponse<PedidoDto>.CreateSuccessResponse(
                    pedidosPaginados,
                    page,
                    pageSize,
                    totalRegistros,
                    "Pedidos obtenidos exitosamente"
                );

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Error interno del servidor: {ex.Message}",
                    Data = null
                });
            }
        }

        /// <summary>
        /// Obtener un pedido por ID
        /// </summary>
        /// <param name="id">ID del pedido</param>
        /// <returns></returns>
        [HttpGet("{id:int}")]
        [PermissionRequired("Pedidos", "VerDetalle")]
        public async Task<IActionResult> GetPedido(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "El ID del pedido debe ser mayor a 0",
                        Data = null
                    });
                }

                var pedido = await _pedidoRepo.GetPedido(id);
                if (pedido == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Pedido no encontrado",
                        Data = null
                    });
                }

                return Ok(new ApiResponse<PedidoDto>
                {
                    Success = true,
                    Message = "Pedido encontrado exitosamente",
                    Data = pedido
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Error interno del servidor: {ex.Message}",
                    Data = null
                });
            }
        }

        /// <summary>
        /// Crear un nuevo pedido con sus detalles
        /// </summary>
        /// <param name="pedidoCrearDto">Datos del pedido a crear</param>
        /// <returns></returns>
        [HttpPost]
        [PermissionRequired("Pedidos", "Crear")]
        public async Task<IActionResult> CrearPedido([FromBody] PedidoCrearDto pedidoCrearDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var usuarioActual = User.FindFirst(ClaimTypes.Name)?.Value ?? "Sistema";

                // Validar que cada ProductoProveedor pertenezca al proveedor del pedido
                foreach (var detalleDto in pedidoCrearDto.Detalles)
                {
                    var pertenece = await _productoProveedorRepo.ProductoProveedorPerteneceAProveedor(
                        detalleDto.IdProductoProveedor, pedidoCrearDto.IdProveedor);
                    if (!pertenece)
                    {
                        return BadRequest(new ApiResponse<object>
                        {
                            Success = false,
                            Message = $"El producto-proveedor con ID {detalleDto.IdProductoProveedor} no pertenece al proveedor seleccionado (ID {pedidoCrearDto.IdProveedor}). Verifique los productos del pedido.",
                            Data = null
                        });
                    }
                }

                var pedido = _mapper.Map<Pedido>(pedidoCrearDto);
                pedido.UsuarioCreacion = usuarioActual;
                pedido.FechaHoraCreacion = DateTime.UtcNow;

                // Asignar usuario y proveedor a los detalles
                foreach (var detalle in pedido.PedidoDetalles)
                {
                    detalle.IdProveedor = pedido.IdProveedor;
                    detalle.UsuarioCreacion = usuarioActual;
                    detalle.FechaHoraCreacion = DateTime.UtcNow;
                }

                var resultado = await _pedidoRepo.CrearPedido(pedido);
                if (!resultado)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "No se pudo crear el pedido",
                        Data = null
                    });
                }

                // Obtener el pedido creado con toda la información
                var pedidoCreado = await _pedidoRepo.GetPedido(pedido.IdPedido);

                return CreatedAtAction(nameof(GetPedido), new { id = pedido.IdPedido }, 
                    new ApiResponse<PedidoDto>
                    {
                        Success = true,
                        Message = "Pedido creado exitosamente",
                        Data = pedidoCreado
                    });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Error interno del servidor: {ex.Message}",
                    Data = null
                });
            }
        }

        /// <summary>
        /// Actualizar un pedido existente
        /// </summary>
        /// <param name="id">ID del pedido</param>
        /// <param name="pedidoActualizarDto">Datos del pedido a actualizar</param>
        /// <returns></returns>
        [HttpPut("{id:int}")]
        [PermissionRequired("Pedidos", "Editar")]
        public async Task<IActionResult> ActualizarPedido(int id, [FromBody] PedidoActualizarDto pedidoActualizarDto)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "El ID del pedido debe ser mayor a 0",
                        Data = null
                    });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var existePedido = await _pedidoRepo.ExistePedido(id);
                if (!existePedido)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Pedido no encontrado",
                        Data = null
                    });
                }

                var usuarioActual = User.FindFirst(ClaimTypes.Name)?.Value ?? "Sistema";

                // Validar que cada ProductoProveedor pertenezca al proveedor del pedido
                foreach (var detalleDto in pedidoActualizarDto.Detalles)
                {
                    var pertenece = await _productoProveedorRepo.ProductoProveedorPerteneceAProveedor(
                        detalleDto.IdProductoProveedor, pedidoActualizarDto.IdProveedor);
                    if (!pertenece)
                    {
                        return BadRequest(new ApiResponse<object>
                        {
                            Success = false,
                            Message = $"El producto-proveedor con ID {detalleDto.IdProductoProveedor} no pertenece al proveedor seleccionado (ID {pedidoActualizarDto.IdProveedor}). Verifique los productos del pedido.",
                            Data = null
                        });
                    }
                }

                var pedido = _mapper.Map<Pedido>(pedidoActualizarDto);
                pedido.IdPedido = id;
                pedido.UsuarioActualizacion = usuarioActual;
                pedido.FechaHoraActualizacion = DateTime.UtcNow;

                // Asignar usuario y proveedor a los detalles
                foreach (var detalle in pedido.PedidoDetalles)
                {
                    detalle.IdPedido = id;
                    detalle.IdProveedor = pedido.IdProveedor;
                    detalle.UsuarioCreacion = usuarioActual;
                    detalle.FechaHoraCreacion = DateTime.UtcNow;
                }

                var resultado = await _pedidoRepo.ActualizarPedido(pedido);
                if (!resultado)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "No se pudo actualizar el pedido",
                        Data = null
                    });
                }

                // Obtener el pedido actualizado con toda la información
                var pedidoActualizado = await _pedidoRepo.GetPedido(id);

                return Ok(new ApiResponse<PedidoDto>
                {
                    Success = true,
                    Message = "Pedido actualizado exitosamente",
                    Data = pedidoActualizado
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Error interno del servidor: {ex.Message}",
                    Data = null
                });
            }
        }


        /// <summary>
        /// Cambiar el estado de un pedido (anular/activar)
        /// </summary>
        /// <param name="id">ID del pedido</param>
        /// <param name="activar">true para activar, false para anular</param>
        /// <returns></returns>
        [HttpPatch("{id:int}/estado")]
        [PermissionRequired("Pedidos", "Anular-Habilitar")]
        public async Task<IActionResult> CambiarEstadoRegistro(int id, [FromQuery] bool activar = false)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "El ID del pedido debe ser mayor a 0",
                        Data = null
                    });
                }

                // Obtener el pedido actual
                var pedido = await _pedidoRepo.GetPedido(id);
                if (pedido == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Pedido no encontrado",
                        Data = null
                    });
                }

                // Obtener IDs de estados
                var idEstadoRegistrado = await GetIdEstadoPorCodigo("RE");
                var idEstadoAnulado = await GetIdEstadoPorCodigo("AN");

                if (idEstadoRegistrado == null || idEstadoAnulado == null)
                {
                    return StatusCode(500, new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Error: No se encontraron los estados requeridos en el sistema",
                        Data = null
                    });
                }

                // Validar que el pedido esté en estado Registrado o Anulado
                if (pedido.IdEstado != idEstadoRegistrado && pedido.IdEstado != idEstadoAnulado)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Solo se puede anular o activar pedidos que estén en estado Registrado o Anulado",
                        Data = null
                    });
                }

                // Validar lógica de cambio de estado
                if (!activar && pedido.IdEstado == idEstadoAnulado)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "El pedido ya está anulado",
                        Data = null
                    });
                }

                if (activar && pedido.IdEstado == idEstadoRegistrado)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "El pedido ya está activo",
                        Data = null
                    });
                }

                var usuarioActual = User.FindFirst(ClaimTypes.Name)?.Value ?? "Sistema";

                // Cambiar estado según el parámetro activar
                bool resultado;
                string mensaje;
                
                if (activar)
                {
                    // Activar pedido (cambiar de Anulado a Registrado)
                    resultado = await _pedidoRepo.CambiarEstadoPedido(id, idEstadoRegistrado.Value, usuarioActual);
                    mensaje = "Pedido activado exitosamente";
                }
                else
                {
                    // Anular pedido (cambiar de Registrado a Anulado)
                    resultado = await _pedidoRepo.AnularPedido(id, usuarioActual);
                    mensaje = "Pedido anulado exitosamente";
                }

                if (!resultado)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"No se pudo {(activar ? "activar" : "anular")} el pedido",
                        Data = null
                    });
                }

                // Obtener el pedido actualizado
                var pedidoActualizado = await _pedidoRepo.GetPedido(id);

                return Ok(new ApiResponse<PedidoDto>
                {
                    Success = true,
                    Message = mensaje,
                    Data = pedidoActualizado
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Error interno del servidor: {ex.Message}",
                    Data = null
                });
            }
        }

        /// <summary>
        /// Distribuir un detalle de pedido por almacenes
        /// </summary>
        /// <param name="distribucionDto">Datos de distribución por almacenes</param>
        /// <returns></returns>
        [HttpPost("distribuir-almacenes")]
        [PermissionRequired("Pedidos", "Editar")]
        public async Task<IActionResult> DistribuirPorAlmacenes([FromBody] DistribucionAlmacenesDto distribucionDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Validar que la distribución tenga sentido
                if (distribucionDto.Distribuciones == null || distribucionDto.Distribuciones.Count == 0)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Debe especificar al menos un almacén para la distribución",
                        Data = null
                    });
                }

                // Validar que no haya almacenes duplicados
                var almacenesDistintos = distribucionDto.Distribuciones.Select(d => d.IdAlmacen).Distinct().Count();
                if (almacenesDistintos != distribucionDto.Distribuciones.Count)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "No se pueden repetir almacenes en la distribución",
                        Data = null
                    });
                }

                // Validar que todas las cantidades sean positivas
                if (distribucionDto.Distribuciones.Any(d => d.Cantidad <= 0))
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Todas las cantidades deben ser mayores a 0",
                        Data = null
                    });
                }

                var usuarioActual = User.FindFirst(ClaimTypes.Name)?.Value ?? "Sistema";

                var resultado = await _pedidoRepo.DistribuirPorAlmacenes(distribucionDto, usuarioActual);
                if (!resultado)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "No se pudo realizar la distribución. Verifique que el detalle del pedido exista y que la suma de cantidades coincida con la cantidad total del detalle.",
                        Data = null
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Distribución por almacenes realizada exitosamente",
                    Data = distribucionDto
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Error interno del servidor: {ex.Message}",
                    Data = null
                });
            }
        }

        // Método auxiliar para obtener ID de estado por código
        private async Task<int?> GetIdEstadoPorCodigo(string codigo)
        {
            return codigo switch
            {
                "RE" => 1, // Registrado
                "VE" => 2, // Verificado
                "TR" => 3, // Trasladado
                "AN" => 4, // Anulado
                _ => null
            };
        }

        /// <summary>
        /// Subir archivo para un pedido
        /// </summary>
        /// <param name="idPedido">ID del pedido</param>
        /// <param name="archivo">Archivo a subir (png, jpg, jpeg, pdf, docx, xlsx, etc.)</param>
        /// <returns></returns>
        [HttpPost("{idPedido:int}/subir-archivo")]
        [PermissionRequired("Pedidos", "Editar")]
        public async Task<IActionResult> SubirArchivo(int idPedido, IFormFile archivo)
        {
            try
            {
                if (idPedido <= 0)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "El ID del pedido debe ser mayor a 0",
                        Data = null
                    });
                }

                if (archivo == null || archivo.Length <= 0)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Debe seleccionar un archivo válido",
                        Data = null
                    });
                }

                var existePedido = await _pedidoRepo.ExistePedido(idPedido);
                if (!existePedido)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Pedido no encontrado",
                        Data = null
                    });
                }

                // Tipos de archivos permitidos
                var tiposPermitidos = new[] { ".png", ".jpg", ".jpeg", ".pdf", ".docx", ".doc", ".xlsx", ".xls", ".txt" };
                var extension = Path.GetExtension(archivo.FileName).ToLower();

                if (!tiposPermitidos.Contains(extension))
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Tipo de archivo no permitido: {extension}. Tipos permitidos: {string.Join(", ", tiposPermitidos)}",
                        Data = null
                    });
                }

                // Validar tamaño (máximo 10MB)
                if (archivo.Length > 10 * 1024 * 1024)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "El archivo excede el tamaño máximo de 10MB",
                        Data = null
                    });
                }

                var usuarioActual = User.FindFirst(ClaimTypes.Name)?.Value ?? "Sistema";

                // Subir archivo usando el servicio
                var rutaArchivo = await _fileStorage.SaveFileAsync(archivo, idPedido);
                
                // Crear registro en base de datos
                var pedidoArchivo = new PedidoArchivo
                {
                    IdPedido = idPedido,
                    NombreArchivo = archivo.FileName,
                    RutaArchivo = rutaArchivo,
                    UsuarioCreacion = usuarioActual,
                    FechaHoraCreacion = DateTime.UtcNow
                };

                var resultado = await _pedidoRepo.AgregarArchivo(pedidoArchivo);
                if (!resultado)
                {
                    // Si falla la BD, eliminar archivo físico
                    await _fileStorage.DeleteFileAsync(rutaArchivo);
                    
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "No se pudo guardar la referencia del archivo en la base de datos",
                        Data = null
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Archivo subido exitosamente",
                    Data = new
                    {
                        idArchivo = pedidoArchivo.IdPedidoArchivo,
                        nombreArchivo = archivo.FileName,
                        rutaArchivo = rutaArchivo,
                        tamanoArchivo = archivo.Length,
                        tipoArchivo = archivo.ContentType
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Error interno del servidor: {ex.Message}",
                    Data = null
                });
            }
        }

        /// <summary>
        /// Obtener archivos de un pedido
        /// </summary>
        /// <param name="idPedido">ID del pedido</param>
        /// <returns></returns>
        [HttpGet("{idPedido:int}/con-archivos")]
        [PermissionRequired("Pedidos", "VerDetalle")]
        public async Task<IActionResult> GetPedidoConArchivos(int idPedido)
        {
            try
            {
                if (idPedido <= 0)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "El ID del pedido debe ser mayor a 0",
                        Data = null
                    });
                }

                var existePedido = await _pedidoRepo.ExistePedido(idPedido);
                if (!existePedido)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Pedido no encontrado",
                        Data = null
                    });
                }

                var archivos = await _pedidoRepo.GetArchivosPedido(idPedido);

                return Ok(new ApiResponse<ICollection<PedidoArchivoDto>>
                {
                    Success = true,
                    Message = "Archivos obtenidos exitosamente",
                    Data = archivos
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Error interno del servidor: {ex.Message}",
                    Data = null
                });
            }
        }

        /// <summary>
        /// Eliminar un archivo de pedido
        /// </summary>
        /// <param name="idArchivo">ID del archivo</param>
        /// <returns></returns>
        [HttpDelete("archivo/{idArchivo:int}")]
        [PermissionRequired("Pedidos", "Editar")]
        public async Task<IActionResult> EliminarArchivo(int idArchivo)
        {
            try
            {
                if (idArchivo <= 0)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "El ID del archivo debe ser mayor a 0",
                        Data = null
                    });
                }

                // Obtener información del archivo antes de eliminarlo
                // Nota: Necesitaríamos un método específico para obtener un archivo por ID
                // Por ahora usaremos una implementación simplificada
                var resultado = await _pedidoRepo.EliminarArchivo(idArchivo);
                if (!resultado)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Archivo no encontrado o no se pudo eliminar",
                        Data = null
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Archivo eliminado exitosamente",
                    Data = null
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Error interno del servidor: {ex.Message}",
                    Data = null
                });
            }
        }

        /// <summary>
        /// Generar URL de vista previa temporal para un archivo de pedido
        /// </summary>
        /// <param name="idArchivo">ID del archivo</param>
        /// <returns>URL temporal para visualizar el archivo</returns>
        [HttpGet("archivo/{idArchivo:int}/preview")]
        [PermissionRequired("Pedidos", "VerDetalle")]
        public async Task<IActionResult> GenerarTokenPreview(int idArchivo)
        {
            try
            {
                if (idArchivo <= 0)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "El ID del archivo debe ser mayor a 0",
                        Data = null
                    });
                }

                var archivo = await _pedidoRepo.GetArchivo(idArchivo);
                if (archivo == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Archivo no encontrado",
                        Data = null
                    });
                }

                // Generar token temporal (válido por 30 minutos)
                var token = GenerarToken(idArchivo);
                var urlPreview = Url.Action("VisualizarArchivo", "Pedidos", new { token = token }, Request.Scheme);

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "URL de vista previa generada exitosamente",
                    Data = new { 
                        url = urlPreview, 
                        token = token,
                        nombreArchivo = archivo.NombreArchivo,
                        expiracion = DateTime.UtcNow.AddMinutes(30).ToString("yyyy-MM-dd HH:mm:ss UTC")
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Error interno del servidor: {ex.Message}",
                    Data = null
                });
            }
        }

        /// <summary>
        /// Visualizar archivo de pedido mediante token temporal
        /// </summary>
        /// <param name="token">Token temporal de acceso</param>
        /// <returns>Archivo para visualización</returns>
        [HttpGet("visualizar")]
        [AllowAnonymous]
        public async Task<IActionResult> VisualizarArchivo(string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Debe proporcionar un token válido para acceder al archivo",
                        Data = null
                    });
                }

                var idArchivo = ValidarToken(token);
                if (idArchivo == 0)
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "El token proporcionado es inválido o ha expirado",
                        Data = null
                    });
                }

                var archivo = await _pedidoRepo.GetArchivo(idArchivo);
                if (archivo == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Archivo no encontrado",
                        Data = null
                    });
                }

                try
                {
                    // Usar el servicio de almacenamiento para obtener el archivo
                    using var fileStream = await _fileStorage.GetFileStreamAsync(archivo.RutaArchivo);
                    var fileBytes = new byte[fileStream.Length];
                    await fileStream.ReadAsync(fileBytes, 0, (int)fileStream.Length);

                    var extension = Path.GetExtension(archivo.NombreArchivo).ToLowerInvariant();
                    var contentType = GetContentType(extension);

                    return File(fileBytes, contentType, archivo.NombreArchivo);
                }
                catch (FileNotFoundException)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "El archivo no existe en el almacenamiento local",
                        Data = null
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Error procesando el archivo: {ex.Message}",
                    Data = null
                });
            }
        }

        #region "Métodos auxiliares"

        private string GenerarToken(int idArchivo)
        {
            var expiracion = DateTimeOffset.UtcNow.AddMinutes(30).ToUnixTimeSeconds();
            var data = $"{idArchivo}:{expiracion}";
            
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes("clave-secreta-para-tokens-pedidos")))
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
                using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes("clave-secreta-para-tokens-pedidos")))
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