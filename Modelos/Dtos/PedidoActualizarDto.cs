using System.ComponentModel.DataAnnotations;

namespace ApiVentas.Modelos.Dtos
{
    public class PedidoActualizarDto
    {
        [Required]
        public int IdPedido { get; set; }

        [Required(ErrorMessage = "El proveedor es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un proveedor válido")]
        public int IdProveedor { get; set; }

        [Required(ErrorMessage = "El estado es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un estado válido")]
        public int IdEstado { get; set; }

        [Required(ErrorMessage = "El monto total es obligatorio")]
        [Range(0, double.MaxValue, ErrorMessage = "El monto total debe ser mayor o igual a 0")]
        public decimal MontoTotal { get; set; }

        [Required(ErrorMessage = "El usuario de actualización es obligatorio")]
        [StringLength(100, ErrorMessage = "El usuario de actualización no puede exceder 100 caracteres")]
        public string UsuarioActualizacion { get; set; }

        // Detalles del pedido
        public List<PedidoDetalleActualizarDto> Detalles { get; set; } = new List<PedidoDetalleActualizarDto>();
    }
}
