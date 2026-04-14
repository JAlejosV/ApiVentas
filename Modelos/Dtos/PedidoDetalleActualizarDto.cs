using System.ComponentModel.DataAnnotations;

namespace ApiVentas.Modelos.Dtos
{
    public class PedidoDetalleActualizarDto
    {
        public int IdPedidoDetalle { get; set; }

        [Required(ErrorMessage = "El proveedor producto es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un proveedor producto válido")]
        public int IdProductoProveedor { get; set; }

        [Required(ErrorMessage = "La cantidad por paquete es obligatoria")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad por paquete debe ser mayor a 0")]
        public int CantidadPorPaquete { get; set; }

        [Required(ErrorMessage = "El precio por paquete es obligatorio")]
        [Range(0, double.MaxValue, ErrorMessage = "El precio por paquete debe ser mayor o igual a 0")]
        public decimal PrecioPorPaquete { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "El pedido por paquete debe ser mayor o igual a 0")]
        public int? PedidoPorPaquete { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "El bono por paquete debe ser mayor o igual a 0")]
        public int? BonoPorPaquete { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "El bono por unidad debe ser mayor o igual a 0")]
        public int? BonoPorUnidad { get; set; }

        [Required(ErrorMessage = "El total de unidades es obligatorio")]
        [Range(0, int.MaxValue, ErrorMessage = "El total de unidades debe ser mayor o igual a 0")]
        public int TotalUnidades { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El precio unitario debe ser mayor o igual a 0")]
        public decimal? PrecioUnitario { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El subtotal debe ser mayor o igual a 0")]
        public decimal? SubTotal { get; set; }

        // Distribución por almacenes
        public List<PedidoDetalleAlmacenActualizarDto> DistribucionAlmacenes { get; set; } = new List<PedidoDetalleAlmacenActualizarDto>();
    }
}
