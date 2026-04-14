using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiVentas.Modelos
{
    public class Producto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdProducto { get; set; }

        [Required]
        [StringLength(30, ErrorMessage = "El código no puede exceder 30 caracteres")]
        public string CodigoProducto { get; set; }

        [Required]
        [StringLength(600, ErrorMessage = "El nombre no puede exceder 600 caracteres")]
        public string NombreProducto { get; set; }

        public int? StockReal { get; set; }

        public int? StockMinimo { get; set; }

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

        // Navegación hacia ProductoProveedor
        public virtual ICollection<ProductoProveedor> ProductoProveedores { get; set; } = new List<ProductoProveedor>();
    }
}
