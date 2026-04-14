using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiVentas.Modelos
{
    public class ProductoProveedor
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdProductoProveedor { get; set; }

        [Required]
        public int IdProveedor { get; set; }

        [Required]
        public int IdProducto { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "La cantidad por paquete debe ser mayor o igual a 0")]
        public int CantidadPorPaquete { get; set; }

        [Required]
        [Column(TypeName = "decimal(16,6)")]
        [Range(0.000001, double.MaxValue, ErrorMessage = "El precio por paquete debe ser mayor a 0")]
        public decimal PrecioPorPaquete { get; set; }

        [Required]
        [Column(TypeName = "decimal(16,6)")]
        [Range(0.000001, double.MaxValue, ErrorMessage = "El precio unitario debe ser mayor a 0")]
        public decimal PrecioUnitario { get; set; }

        [Required]
        public bool EstadoRegistro { get; set; } = true;

        [Required]
        [StringLength(100, ErrorMessage = "El usuario de creación no puede exceder 100 caracteres")]
        public string UsuarioCreacion { get; set; } = "SYSTEM";

        [Required]
        public DateTime FechaHoraCreacion { get; set; } = DateTime.UtcNow;

        [StringLength(100, ErrorMessage = "El usuario de actualización no puede exceder 100 caracteres")]
        public string UsuarioActualizacion { get; set; }

        public DateTime? FechaHoraActualizacion { get; set; }

        // Navegación
        [ForeignKey("IdProveedor")]
        public virtual Proveedor Proveedor { get; set; }

        [ForeignKey("IdProducto")]
        public virtual Producto Producto { get; set; }
    }
}
