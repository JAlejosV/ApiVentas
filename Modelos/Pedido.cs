using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiVentas.Modelos
{
    public class Pedido
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdPedido { get; set; }

        [Required(ErrorMessage = "El proveedor es obligatorio")]
        public int IdProveedor { get; set; }

        [Required(ErrorMessage = "El estado es obligatorio")]
        public int IdEstado { get; set; }

        [Required(ErrorMessage = "El monto total es obligatorio")]
        [Column(TypeName = "decimal(16,6)")]
        [Range(0, double.MaxValue, ErrorMessage = "El monto total debe ser mayor o igual a 0")]
        public decimal MontoTotal { get; set; }

        [Required]
        [StringLength(100)]
        public string UsuarioCreacion { get; set; } = string.Empty;

        [Required]
        public DateTime FechaHoraCreacion { get; set; } = DateTime.Now;

        [StringLength(100)]
        public string UsuarioActualizacion { get; set; }

        public DateTime? FechaHoraActualizacion { get; set; }

        // Navegación
        [ForeignKey("IdProveedor")]
        public virtual Proveedor Proveedor { get; set; }

        [ForeignKey("IdEstado")]
        public virtual Estado Estado { get; set; }

        // Colecciones relacionadas
        public virtual ICollection<PedidoDetalle> PedidoDetalles { get; set; } = new List<PedidoDetalle>();
        public virtual ICollection<PedidoArchivo> PedidoArchivos { get; set; } = new List<PedidoArchivo>();
    }
}
