using ApiVentas.Data;
using ApiVentas.Modelos;
using ApiVentas.Modelos.Dtos;
using ApiVentas.Repositorio;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace ApiVentas.Repositorio
{
    public class ProductoProveedorRepositorio : IProductoProveedorRepositorio
    {
        private readonly ApplicationDbContext _bd;
        private readonly IMapper _mapper;

        public ProductoProveedorRepositorio(ApplicationDbContext bd, IMapper mapper)
        {
            _bd = bd;
            _mapper = mapper;
        }

        public async Task<ICollection<ProductoProveedorDto>> GetProductoProveedores()
        {
            var lista = await _bd.ProductoProveedor
                .Include(pp => pp.Proveedor)
                .Include(pp => pp.Producto)
                .Where(pp => pp.EstadoRegistro)
                .OrderBy(pp => pp.Proveedor.NombreProveedor)
                .ThenBy(pp => pp.Producto.NombreProducto)
                .ToListAsync();

            return _mapper.Map<ICollection<ProductoProveedorDto>>(lista);
        }

        public async Task<ICollection<ProductoProveedorDto>> GetProductoProveedoresPorProveedor(int idProveedor)
        {
            var lista = await _bd.ProductoProveedor
                .Include(pp => pp.Proveedor)
                .Include(pp => pp.Producto)
                .Where(pp => pp.IdProveedor == idProveedor && pp.EstadoRegistro)
                .OrderBy(pp => pp.Producto.NombreProducto)
                .ToListAsync();

            return _mapper.Map<ICollection<ProductoProveedorDto>>(lista);
        }

        public async Task<ICollection<ProductoProveedorDto>> GetProductoProveedoresPorProducto(int idProducto)
        {
            var lista = await _bd.ProductoProveedor
                .Include(pp => pp.Proveedor)
                .Include(pp => pp.Producto)
                .Where(pp => pp.IdProducto == idProducto && pp.EstadoRegistro)
                .OrderBy(pp => pp.Proveedor.NombreProveedor)
                .ToListAsync();

            return _mapper.Map<ICollection<ProductoProveedorDto>>(lista);
        }

        public async Task<ProductoProveedorDto> GetProductoProveedor(int id)
        {
            var productoProveedor = await _bd.ProductoProveedor
                .Include(pp => pp.Proveedor)
                .Include(pp => pp.Producto)
                .FirstOrDefaultAsync(pp => pp.IdProductoProveedor == id);

            return _mapper.Map<ProductoProveedorDto>(productoProveedor);
        }

        public async Task<bool> ExisteProductoProveedor(int id)
        {
            return await _bd.ProductoProveedor.AnyAsync(pp => pp.IdProductoProveedor == id);
        }

        public async Task<bool> ExisteProductoProveedor(int idProveedor, int idProducto)
        {
            return await _bd.ProductoProveedor.AnyAsync(pp => 
                pp.IdProveedor == idProveedor && 
                pp.IdProducto == idProducto && 
                pp.EstadoRegistro);
        }

        public async Task<bool> ProductoProveedorPerteneceAProveedor(int idProductoProveedor, int idProveedor)
        {
            return await _bd.ProductoProveedor.AnyAsync(pp =>
                pp.IdProductoProveedor == idProductoProveedor &&
                pp.IdProveedor == idProveedor &&
                pp.EstadoRegistro);
        }

        public async Task<bool> CrearProductoProveedor(ProductoProveedor productoProveedor)
        {
            _bd.ProductoProveedor.Add(productoProveedor);
            return await Guardar();
        }

        public async Task<bool> ActualizarProductoProveedor(ProductoProveedor productoProveedor)
        {
            _bd.ProductoProveedor.Update(productoProveedor);
            return await Guardar();
        }

        public async Task<bool> EliminarProductoProveedor(int id)
        {
            var productoProveedor = await _bd.ProductoProveedor.FindAsync(id);
            if (productoProveedor == null)
                return false;

            productoProveedor.EstadoRegistro = false;
            productoProveedor.UsuarioActualizacion = "SYSTEM";
            productoProveedor.FechaHoraActualizacion = DateTime.UtcNow;
            return await Guardar();
        }

        public async Task<bool> Guardar()
        {
            return await _bd.SaveChangesAsync() >= 0;
        }
    }
}
