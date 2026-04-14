using System.ComponentModel.DataAnnotations;

namespace ApiVentas.Modelos.Dtos
{
    public class PedidoDetalleDto
    {
        public int IdPedidoDetalle { get; set; }
        public int IdPedido { get; set; }
        public int IdProveedor { get; set; }
        public int IdProductoProveedor { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public int CantidadPorPaquete { get; set; }
        public decimal PrecioPorPaquete { get; set; }
        public int? PedidoPorPaquete { get; set; }
        public int? BonoPorPaquete { get; set; }
        public int? BonoPorUnidad { get; set; }
        public int TotalUnidades { get; set; }
        public decimal? PrecioUnitario { get; set; }
        public decimal? SubTotal { get; set; }
        public string UsuarioCreacion { get; set; } = string.Empty;
        public DateTime FechaHoraCreacion { get; set; }
        public string UsuarioActualizacion { get; set; }
        public DateTime? FechaHoraActualizacion { get; set; }
        public List<PedidoDetalleAlmacenDto> PedidoDetalleAlmacenes { get; set; } = new List<PedidoDetalleAlmacenDto>();
    }
}
