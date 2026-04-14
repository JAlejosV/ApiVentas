using ApiVentas.Modelos;
using ApiVentas.Modelos.Dtos;

namespace ApiVentas.Repositorio.IRepositorio
{
    public interface IPedidoRepositorio
    {
        Task<ICollection<PedidoDto>> GetPedidos();
        Task<ICollection<PedidoDto>> BuscarPedidos(PedidoFiltroDto filtro);
        Task<PedidoDto> GetPedido(int id);
        Task<bool> ExistePedido(int id);
        Task<bool> CrearPedido(Pedido pedido);
        Task<bool> ActualizarPedido(Pedido pedido);
        Task<bool> VerificarPedido(int id, string usuario, decimal? montoVerificacion = null);
        Task<bool> TrasladarPedido(int id, string usuario);
        Task<bool> AnularPedido(int id, string usuario);
        Task<bool> ReactivarPedido(int id, string usuario);
        Task<bool> CambiarEstadoPedido(int idPedido, int idEstado, string usuario);
        
        // Gestión de archivos
        Task<bool> AgregarArchivo(PedidoArchivo archivo);
        Task<bool> EliminarArchivo(int idArchivo);
        Task<ICollection<PedidoArchivoDto>> GetArchivosPedido(int idPedido);
        Task<PedidoArchivoDto> GetArchivo(int idArchivo);
        
        // Distribución por almacenes
        Task<bool> DistribuirPorAlmacenes(DistribucionAlmacenesDto distribucion, string usuario);
        
        // Validaciones de negocio
        Task<bool> ValidarMontoVerificacion(int idPedido, decimal montoVerificacion);
        Task<decimal> CalcularMontoTotal(int idPedido);
        
        Task<bool> Guardar();
    }
}
