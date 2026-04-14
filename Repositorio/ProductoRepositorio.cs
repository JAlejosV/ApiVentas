using ApiVentas.Data;
using ApiVentas.Modelos;
using ApiVentas.Modelos.Dtos;
using ApiVentas.Repositorio.IRepositorio;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace ApiVentas.Repositorio
{
    public class ProductoRepositorio : IProductoRepositorio
    {
        private readonly ApplicationDbContext _bd;
        private readonly IMapper _mapper;

        public ProductoRepositorio(ApplicationDbContext bd, IMapper mapper)
        {
            _bd = bd;
            _mapper = mapper;
        }

        public async Task<ICollection<ProductoDto>> GetProductos()
        {
            try
            {
                // Optimización: Cargar todo en una sola query con Include
                var productos = await _bd.Producto
                    .Include(p => p.ProductoProveedores.Where(pp => pp.EstadoRegistro))
                        .ThenInclude(pp => pp.Proveedor)
                    .OrderBy(p => p.NombreProducto)
                    .AsSplitQuery() // Optimiza queries con múltiples includes
                    .ToListAsync();

                var productosDto = productos.Select(producto => 
                {
                    var proveedoresDto = producto.ProductoProveedores
                        .Select(pp => new ProductoProveedorDto
                        {
                            IdProductoProveedor = pp.IdProductoProveedor,
                            IdProveedor = pp.IdProveedor,
                            IdProducto = pp.IdProducto,
                            NombreProveedor = pp.Proveedor.NombreProveedor,
                            CantidadPorPaquete = pp.CantidadPorPaquete,
                            PrecioPorPaquete = pp.PrecioPorPaquete,
                            PrecioUnitario = pp.PrecioUnitario,
                            EstadoRegistro = pp.EstadoRegistro
                        }).ToList();

                    return new ProductoDto
                    {
                        IdProducto = producto.IdProducto,
                        CodigoProducto = producto.CodigoProducto,
                        NombreProducto = producto.NombreProducto,
                        StockReal = producto.StockReal,
                        StockMinimo = producto.StockMinimo,
                        EstadoRegistro = producto.EstadoRegistro,
                        Proveedores = proveedoresDto,
                        ProveedoresFormateado = proveedoresDto.Any()
                            ? string.Join(", ", proveedoresDto.Select(p => 
                                $"{p.NombreProveedor} (S/{p.PrecioUnitario.ToString("F2", CultureInfo.InvariantCulture)})"))
                            : "Sin proveedores configurados"
                    };
                }).ToList();

                return productosDto;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error en GetProductos: {ex.Message}", ex);
            }
        }

        public async Task<ICollection<ProductoDto>> GetProductosPorProveedor(int idProveedor)
        {
            var productos = await _bd.Producto
                .Include(p => p.ProductoProveedores)
                    .ThenInclude(pp => pp.Proveedor)
                .Where(p => p.EstadoRegistro && p.ProductoProveedores.Any(pp => pp.IdProveedor == idProveedor && pp.EstadoRegistro))
                .OrderBy(p => p.NombreProducto)
                .ToListAsync();

            var productosDto = new List<ProductoDto>();

            foreach (var producto in productos)
            {
                var productoDto = new ProductoDto
                {
                    IdProducto = producto.IdProducto,
                    CodigoProducto = producto.CodigoProducto,
                    NombreProducto = producto.NombreProducto,
                    StockReal = producto.StockReal,
                    StockMinimo = producto.StockMinimo,
                    EstadoRegistro = producto.EstadoRegistro,
                    Proveedores = new List<ProductoProveedorDto>()
                };

                // Solo incluir el proveedor específico
                var productoProveedor = producto.ProductoProveedores.FirstOrDefault(pp => pp.IdProveedor == idProveedor && pp.EstadoRegistro);
                if (productoProveedor != null)
                {
                    productoDto.Proveedores.Add(new ProductoProveedorDto
                    {
                        IdProductoProveedor = productoProveedor.IdProductoProveedor,
                        IdProveedor = productoProveedor.IdProveedor,
                        IdProducto = productoProveedor.IdProducto,
                        NombreProveedor = productoProveedor.Proveedor.NombreProveedor,
                        CantidadPorPaquete = productoProveedor.CantidadPorPaquete,
                        PrecioPorPaquete = productoProveedor.PrecioPorPaquete,
                        PrecioUnitario = productoProveedor.PrecioUnitario,
                        EstadoRegistro = productoProveedor.EstadoRegistro
                    });

                    productoDto.ProveedoresFormateado = $"{productoProveedor.Proveedor.NombreProveedor} (S/{productoProveedor.PrecioUnitario.ToString("F2", CultureInfo.InvariantCulture)})";
                }

                productosDto.Add(productoDto);
            }

            return productosDto;
        }

        public async Task<ProductoDto> GetProducto(int id)
        {
            var producto = await _bd.Producto
                .Include(p => p.ProductoProveedores)
                    .ThenInclude(pp => pp.Proveedor)
                .FirstOrDefaultAsync(p => p.IdProducto == id);

            if (producto == null)
                return null;

            var productoDto = new ProductoDto
            {
                IdProducto = producto.IdProducto,
                CodigoProducto = producto.CodigoProducto,
                NombreProducto = producto.NombreProducto,
                StockReal = producto.StockReal,
                StockMinimo = producto.StockMinimo,
                EstadoRegistro = producto.EstadoRegistro,
                Proveedores = new List<ProductoProveedorDto>()
            };

            foreach (var pp in producto.ProductoProveedores)
            {
                productoDto.Proveedores.Add(new ProductoProveedorDto
                {
                    IdProductoProveedor = pp.IdProductoProveedor,
                    IdProveedor = pp.IdProveedor,
                    IdProducto = pp.IdProducto,
                    NombreProveedor = pp.Proveedor.NombreProveedor,
                    CantidadPorPaquete = pp.CantidadPorPaquete,
                    PrecioPorPaquete = pp.PrecioPorPaquete,
                    PrecioUnitario = pp.PrecioUnitario,
                    EstadoRegistro = pp.EstadoRegistro
                });
            }

            productoDto.ProveedoresFormateado = string.Join(", ", 
                productoDto.Proveedores.Select(p => 
                    $"{p.NombreProveedor} (S/{p.PrecioUnitario.ToString("F2", CultureInfo.InvariantCulture)})"));

            return productoDto;
        }

        public async Task<bool> ExisteProducto(int id)
        {
            return await _bd.Producto.AnyAsync(p => p.IdProducto == id);
        }

        public async Task<bool> ExisteProductoPorCodigo(string codigo)
        {
            return await _bd.Producto.AnyAsync(p => p.CodigoProducto.ToLower().Trim() == codigo.ToLower().Trim());
        }

        public async Task<bool> ExisteProductoPorCodigo(string codigo, int? idProductoExcluir)
        {
            return await _bd.Producto.AnyAsync(p => 
                p.CodigoProducto.ToLower().Trim() == codigo.ToLower().Trim() && 
                (!idProductoExcluir.HasValue || p.IdProducto != idProductoExcluir.Value));
        }

        public async Task<int?> CrearProducto(ProductoCrearDto productoDto)
        {
            try
            {
                // Validar que al menos hay un proveedor
                if (productoDto.Proveedores == null || !productoDto.Proveedores.Any())
                {
                    throw new ArgumentException("Debe especificar al menos un proveedor");
                }

                // Crear el producto
                var producto = new Producto
                {
                    CodigoProducto = productoDto.CodigoProducto,
                    NombreProducto = productoDto.NombreProducto,
                    StockReal = productoDto.StockReal,
                    StockMinimo = productoDto.StockMinimo,
                    EstadoRegistro = true,
                    UsuarioCreacion = "SYSTEM", // TODO: Obtener del usuario actual
                    FechaHoraCreacion = DateTime.UtcNow
                };

                _bd.Producto.Add(producto);
                await _bd.SaveChangesAsync();

                // Agregar proveedores
                foreach (var proveedorDto in productoDto.Proveedores)
                {
                    try
                    {
                        var productoProveedor = new ProductoProveedor
                        {
                            IdProveedor = proveedorDto.IdProveedor,
                            IdProducto = producto.IdProducto,
                            CantidadPorPaquete = proveedorDto.CantidadPorPaquete,
                            PrecioPorPaquete = proveedorDto.PrecioPorPaquete,
                            PrecioUnitario = proveedorDto.PrecioUnitario,
                            EstadoRegistro = proveedorDto.EstadoRegistro,
                            UsuarioCreacion = "SYSTEM",
                            FechaHoraCreacion = DateTime.UtcNow
                        };

                        _bd.ProductoProveedor.Add(productoProveedor);
                    }
                    catch (Exception provEx)
                    {
                        throw new Exception($"Error al agregar proveedor {proveedorDto.IdProveedor}: {provEx.Message}", provEx);
                    }
                }

                await _bd.SaveChangesAsync();
                return producto.IdProducto;
            }
            catch (Exception ex)
            {
                // Log the error for debugging
                throw new Exception($"Error al crear producto: {ex.Message}", ex);
            }
        }

        public async Task<bool> ActualizarProducto(ProductoActualizarDto productoDto)
        {
            try
            {
                var producto = await _bd.Producto.FindAsync(productoDto.IdProducto);
                if (producto == null)
                    return false;

                // Actualizar el producto
                producto.CodigoProducto = productoDto.CodigoProducto;
                producto.NombreProducto = productoDto.NombreProducto;
                producto.StockReal = productoDto.StockReal;
                producto.StockMinimo = productoDto.StockMinimo;
                producto.EstadoRegistro = productoDto.EstadoRegistro;
                producto.UsuarioActualizacion = "SYSTEM"; // TODO: Obtener del usuario actual
                producto.FechaHoraActualizacion = DateTime.UtcNow;

                await _bd.SaveChangesAsync();

                // Obtener proveedores existentes
                var proveedoresExistentes = await _bd.ProductoProveedor
                    .Where(pp => pp.IdProducto == productoDto.IdProducto)
                    .ToListAsync();

                // Procesar proveedores
                foreach (var proveedorDto in productoDto.Proveedores)
                {
                    if (proveedorDto.IdProductoProveedor.HasValue)
                    {
                        // Actualizar existente
                        var proveedorExistente = proveedoresExistentes
                            .FirstOrDefault(pe => pe.IdProductoProveedor == proveedorDto.IdProductoProveedor.Value);
                        if (proveedorExistente != null)
                        {
                            proveedorExistente.IdProveedor = proveedorDto.IdProveedor;
                            proveedorExistente.CantidadPorPaquete = proveedorDto.CantidadPorPaquete;
                            proveedorExistente.PrecioPorPaquete = proveedorDto.PrecioPorPaquete;
                            proveedorExistente.PrecioUnitario = proveedorDto.PrecioUnitario;
                            proveedorExistente.EstadoRegistro = proveedorDto.EstadoRegistro;
                            proveedorExistente.UsuarioActualizacion = "SYSTEM";
                            proveedorExistente.FechaHoraActualizacion = DateTime.UtcNow;
                        }
                    }
                    else
                    {
                        // Crear nuevo
                        var nuevoProductoProveedor = new ProductoProveedor
                        {
                            IdProveedor = proveedorDto.IdProveedor,
                            IdProducto = productoDto.IdProducto,
                            CantidadPorPaquete = proveedorDto.CantidadPorPaquete,
                            PrecioPorPaquete = proveedorDto.PrecioPorPaquete,
                            PrecioUnitario = proveedorDto.PrecioUnitario,
                            EstadoRegistro = proveedorDto.EstadoRegistro,
                            UsuarioCreacion = "SYSTEM",
                            FechaHoraCreacion = DateTime.UtcNow
                        };

                        _bd.ProductoProveedor.Add(nuevoProductoProveedor);
                    }
                }

                await _bd.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al actualizar producto: {ex.Message}", ex);
            }
        }

        public async Task<bool> AnularProducto(int id)
        {
            var producto = await _bd.Producto.FindAsync(id);
            if (producto == null)
                return false;

            producto.EstadoRegistro = false;
            producto.UsuarioActualizacion = "SYSTEM";
            producto.FechaHoraActualizacion = DateTime.UtcNow;
            return await Guardar();
        }

        public async Task<bool> HabilitarProducto(int id)
        {
            var producto = await _bd.Producto.FindAsync(id);
            if (producto == null)
                return false;

            producto.EstadoRegistro = true;
            producto.UsuarioActualizacion = "SYSTEM";
            producto.FechaHoraActualizacion = DateTime.UtcNow;
            return await Guardar();
        }

        public async Task<ICollection<ProductoProveedorDto>> GetProveedoresDeProducto(int idProducto)
        {
            var proveedores = await _bd.ProductoProveedor
                .Include(pp => pp.Proveedor)
                .Where(pp => pp.IdProducto == idProducto)
                .ToListAsync();

            return _mapper.Map<ICollection<ProductoProveedorDto>>(proveedores);
        }

        public async Task<bool> AgregarProveedorAProducto(int idProducto, ProductoProveedorCrearDto ProductoProveedorDto)
        {
            var productoProveedor = new ProductoProveedor
            {
                IdProveedor = ProductoProveedorDto.IdProveedor,
                IdProducto = idProducto,
                CantidadPorPaquete = ProductoProveedorDto.CantidadPorPaquete,
                PrecioPorPaquete = ProductoProveedorDto.PrecioPorPaquete,
                PrecioUnitario = ProductoProveedorDto.PrecioUnitario,
                EstadoRegistro = ProductoProveedorDto.EstadoRegistro,
                UsuarioCreacion = "SYSTEM",
                FechaHoraCreacion = DateTime.UtcNow
            };

            _bd.ProductoProveedor.Add(productoProveedor);
            return await Guardar();
        }

        public async Task<bool> ActualizarProveedorDeProducto(ProductoProveedorActualizarDto productoProveedorDto)
        {
            var productoProveedor = await _bd.ProductoProveedor.FindAsync(productoProveedorDto.IdProductoProveedor);
            if (productoProveedor == null)
                return false;

            productoProveedor.IdProveedor = productoProveedorDto.IdProveedor;
            productoProveedor.CantidadPorPaquete = productoProveedorDto.CantidadPorPaquete;
            productoProveedor.PrecioPorPaquete = productoProveedorDto.PrecioPorPaquete;
            productoProveedor.PrecioUnitario = productoProveedorDto.PrecioUnitario;
            productoProveedor.EstadoRegistro = productoProveedorDto.EstadoRegistro;
            productoProveedor.UsuarioActualizacion = "SYSTEM";
            productoProveedor.FechaHoraActualizacion = DateTime.UtcNow;

            return await Guardar();
        }

        public async Task<bool> EliminarProveedorDeProducto(int IdProductoProveedor)
        {
            var productoProveedor = await _bd.ProductoProveedor.FindAsync(IdProductoProveedor);
            if (productoProveedor == null)
                return false;

            productoProveedor.EstadoRegistro = false;
            productoProveedor.UsuarioActualizacion = "SYSTEM";
            productoProveedor.FechaHoraActualizacion = DateTime.UtcNow;

            return await Guardar();
        }

        public async Task<bool> ExisteProductoProveedor(int IdProductoProveedor)
        {
            return await _bd.ProductoProveedor.AnyAsync(pp => pp.IdProductoProveedor == IdProductoProveedor);
        }

        public async Task<bool> ActualizarProductoProveedor(int IdProductoProveedor, int cantidadPorPaquete, decimal precioPorPaquete, decimal precioUnitario)
        {
            var productoProveedor = await _bd.ProductoProveedor.FirstOrDefaultAsync(pp => pp.IdProductoProveedor == IdProductoProveedor);
            
            if (productoProveedor == null)
                return false;

            productoProveedor.CantidadPorPaquete = cantidadPorPaquete;
            productoProveedor.PrecioPorPaquete = precioPorPaquete;
            productoProveedor.PrecioUnitario = precioUnitario;
            productoProveedor.FechaHoraActualizacion = DateTime.UtcNow;

            return await Guardar();
        }

        public async Task<bool> Guardar()
        {
            return await _bd.SaveChangesAsync() >= 0;
        }
    }
}
