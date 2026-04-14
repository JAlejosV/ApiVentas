using System.ComponentModel.DataAnnotations;

namespace ApiVentas.Modelos.Dtos
{
    public class PedidoDetalleAlmacenActualizarDto
    {
        public int IdPedidoDetalleAlmacen { get; set; }

        [Required(ErrorMessage = "El detalle del pedido es obligatorio")]
        public int IdPedidoDetalle { get; set; }

        [Required(ErrorMessage = "El almacén es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un almacén válido")]
        public int IdAlmacen { get; set; }

        [Required(ErrorMessage = "La cantidad es obligatoria")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        public int Cantidad { get; set; }
    }
}
