using ApiVentas.Modelos;
using ApiVentas.Modelos.Dtos;

namespace ApiVentas.Repositorio
{
    public interface IProductoProveedorRepositorio
    {
        Task<ICollection<ProductoProveedorDto>> GetProductoProveedores();
        Task<ICollection<ProductoProveedorDto>> GetProductoProveedoresPorProveedor(int idProveedor);
        Task<ICollection<ProductoProveedorDto>> GetProductoProveedoresPorProducto(int idProducto);
        Task<ProductoProveedorDto> GetProductoProveedor(int id);
        Task<bool> ExisteProductoProveedor(int id);
        Task<bool> ExisteProductoProveedor(int idProveedor, int idProducto);
        Task<bool> ProductoProveedorPerteneceAProveedor(int idProductoProveedor, int idProveedor);
        Task<bool> CrearProductoProveedor(ProductoProveedor productoProveedor);
        Task<bool> ActualizarProductoProveedor(ProductoProveedor productoProveedor);
        Task<bool> EliminarProductoProveedor(int id);
        Task<bool> Guardar();
    }
}
