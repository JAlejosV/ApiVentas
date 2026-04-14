using ApiVentas.Modelos;
using ApiVentas.Modelos.Dtos;

namespace ApiVentas.Repositorio.IRepositorio
{
    public interface IPedidoDetalleAlmacenRepositorio
    {
        Task<ICollection<PedidoDetalleAlmacenDto>> GetDistribucionPorDetalle(int idPedidoDetalle);
        Task<bool> CrearDistribucion(List<PedidoDetalleAlmacen> distribuciones);
        Task<bool> ActualizarDistribucion(int idPedidoDetalle, List<PedidoDetalleAlmacen> nuevasDistribuciones);
        Task<bool> EliminarDistribucionDetalle(int idPedidoDetalle);
        Task<bool> ValidarDistribucion(int idPedidoDetalle, List<PedidoDetalleAlmacenCrearDto> distribuciones);
        Task<int> GetCantidadTotalDistribuida(int idPedidoDetalle);
        Task<bool> Guardar();
    }
}
