using ApiVentas.Modelos;
using ApiVentas.Modelos.Dtos;

namespace ApiVentas.Repositorio.IRepositorio
{
    public interface IProveedorRepositorio
    {
        // Métodos existentes
        ICollection<Proveedor> GetProveedores();
        Proveedor GetProveedor(int idProveedor);
        bool ExisteProveedor(string nombreProveedor);
        bool ExisteProveedor(int idProveedor);
        bool ExisteProveedorExcluyendoId(string nombreProveedor, int idProveedor);
        bool ExisteCodigoProveedor(int codigoProveedor);
        bool ExisteCodigoProveedorExcluyendoId(int codigoProveedor, int idProveedor);
        bool CrearProveedor(Proveedor proveedor);
        bool ActualizarProveedor(Proveedor proveedor);
        Task<bool> AnularProveedor(int idProveedor);
        Task<bool> HabilitarProveedor(int idProveedor);
        bool Guardar();

        // Métodos async adicionales
        Task<bool> ExisteProveedorAsync(int idProveedor);
        Task<bool> ExisteProveedorAsync(string nombreProveedor);
    }
}
