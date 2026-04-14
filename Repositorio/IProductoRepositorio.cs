using ApiVentas.Modelos;
using ApiVentas.Modelos.Dtos;

namespace ApiVentas.Repositorio.IRepositorio
{
    public interface IProductoRepositorio
    {
        // Métodos existentes que ya están implementados
        Task<ICollection<ProductoDto>> GetProductos();
        Task<ICollection<ProductoDto>> GetProductosPorProveedor(int idProveedor);
        Task<ProductoDto> GetProducto(int id);
        Task<bool> ExisteProducto(int id);
        Task<bool> ExisteProductoPorCodigo(string codigo);
        Task<bool> ExisteProductoPorCodigo(string codigo, int? idProductoExcluir);
        Task<int?> CrearProducto(ProductoCrearDto productoDto);
        Task<bool> ActualizarProducto(ProductoActualizarDto productoDto);
        Task<bool> AnularProducto(int id);
        Task<bool> HabilitarProducto(int id);
        Task<bool> Guardar();
        
        // Métodos para manejar ProductoProveedor
        Task<ICollection<ProductoProveedorDto>> GetProveedoresDeProducto(int idProducto);
        Task<bool> AgregarProveedorAProducto(int idProducto, ProductoProveedorCrearDto productoProveedorDto);
        Task<bool> ActualizarProveedorDeProducto(ProductoProveedorActualizarDto productoProveedorDto);
        Task<bool> EliminarProveedorDeProducto(int idProductoProveedor);
        Task<bool> ExisteProductoProveedor(int idProductoProveedor);
        Task<bool> ActualizarProductoProveedor(int idProductoProveedor, int cantidadPorPaquete, decimal precioPorPaquete, decimal precioUnitario);
    }
}
