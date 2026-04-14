using System.ComponentModel.DataAnnotations;

namespace ApiVentas.Modelos.Dtos
{
    public class DistribucionAlmacenesDto
    {
        [Required(ErrorMessage = "El ID del detalle del pedido es obligatorio")]
        public int IdPedidoDetalle { get; set; }

        [Required(ErrorMessage = "La distribución de almacenes es obligatoria")]
        [MinLength(1, ErrorMessage = "Debe incluir al menos un almacén")]
        public List<PedidoDetalleAlmacenCrearDto> Distribuciones { get; set; } = new();
    }
}
