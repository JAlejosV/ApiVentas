using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiVentas.Modelos
{
    public class PedidoDetalle
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdPedidoDetalle { get; set; }

        [Required(ErrorMessage = "El pedido es obligatorio")]
        public int IdPedido { get; set; }

        [Required(ErrorMessage = "El proveedor es obligatorio")]
        public int IdProveedor { get; set; }

        [Required(ErrorMessage = "El proveedor producto es obligatorio")]
        public int IdProductoProveedor { get; set; }

        [Required(ErrorMessage = "La cantidad por paquete es obligatoria")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad por paquete debe ser mayor a 0")]
        public int CantidadPorPaquete { get; set; }

        [Required(ErrorMessage = "El precio por paquete es obligatorio")]
        [Column(TypeName = "decimal(16,6)")]
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

        [Column(TypeName = "decimal(16,6)")]
        [Range(0, double.MaxValue, ErrorMessage = "El precio unitario debe ser mayor o igual a 0")]
        public decimal? PrecioUnitario { get; set; }

        [Column(TypeName = "decimal(16,6)")]
        [Range(0, double.MaxValue, ErrorMessage = "El subtotal debe ser mayor o igual a 0")]
        public decimal? SubTotal { get; set; }

        [Required]
        [StringLength(100)]
        public string UsuarioCreacion { get; set; } = string.Empty;

        [Required]
        public DateTime FechaHoraCreacion { get; set; } = DateTime.Now;

        [StringLength(100)]
        public string UsuarioActualizacion { get; set; }

        public DateTime? FechaHoraActualizacion { get; set; }

        // Navegación
        [ForeignKey("IdPedido")]
        public virtual Pedido Pedido { get; set; }

        [ForeignKey("IdProductoProveedor")]
        public virtual ProductoProveedor ProductoProveedor { get; set; }

        // Colecciones relacionadas
        public virtual ICollection<PedidoDetalleAlmacen> PedidoDetalleAlmacenes { get; set; } = new List<PedidoDetalleAlmacen>();
    }
}
