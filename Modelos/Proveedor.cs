using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiVentas.Modelos
{
    public class Proveedor
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdProveedor { get; set; }
        
        [Required(ErrorMessage = "El código del proveedor es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "El código del proveedor debe ser un número válido mayor a 0")]
        public int CodigoProveedor { get; set; }
        
        [Required(ErrorMessage = "El nombre del proveedor es obligatorio")]
        [StringLength(150, ErrorMessage = "El nombre no puede exceder 150 caracteres")]
        public string NombreProveedor { get; set; }
        
        [StringLength(50, ErrorMessage = "El teléfono no puede exceder 50 caracteres")]
        public string Telefono { get; set; }
        
        [StringLength(250, ErrorMessage = "El contacto no puede exceder 250 caracteres")]
        public string Contacto { get; set; }
        
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

        // Navegación hacia los archivos
        public virtual ICollection<ProveedorArchivo> Archivos { get; set; } = new List<ProveedorArchivo>();

        // Navegación hacia ProductoProveedor
        public virtual ICollection<ProductoProveedor> ProductoProveedores { get; set; } = new List<ProductoProveedor>();
    }
}
