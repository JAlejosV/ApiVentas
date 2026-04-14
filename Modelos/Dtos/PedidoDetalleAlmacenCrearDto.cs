using System.ComponentModel.DataAnnotations;

namespace ApiVentas.Modelos.Dtos
{
    public class PedidoDetalleAlmacenCrearDto
    {
        [Required(ErrorMessage = "El almacén es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un almacén válido")]
        public int IdAlmacen { get; set; }

        [Required(ErrorMessage = "La cantidad es obligatoria")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        public int Cantidad { get; set; }
    }
}
