using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiVentas.Modelos
{
    public class PedidoDetalleAlmacen
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdPedidoDetalleAlmacen { get; set; }

        [Required(ErrorMessage = "El detalle del pedido es obligatorio")]
        public int IdPedidoDetalle { get; set; }

        [Required(ErrorMessage = "El almacén es obligatorio")]
        public int IdAlmacen { get; set; }

        [Required(ErrorMessage = "La cantidad es obligatoria")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        public int Cantidad { get; set; }

        [Required]
        [StringLength(100)]
        public string UsuarioCreacion { get; set; } = string.Empty;

        [Required]
        public DateTime FechaHoraCreacion { get; set; } = DateTime.Now;

        [StringLength(100)]
        public string UsuarioActualizacion { get; set; }

        public DateTime? FechaHoraActualizacion { get; set; }

        // Navegación
        [ForeignKey("IdPedidoDetalle")]
        public virtual PedidoDetalle PedidoDetalle { get; set; }

        [ForeignKey("IdAlmacen")]
        public virtual Almacen Almacen { get; set; }
    }
}
