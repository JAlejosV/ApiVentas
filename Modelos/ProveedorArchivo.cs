using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiVentas.Modelos
{
    public class ProveedorArchivo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdProveedorArchivo { get; set; }
        
        [Required]
        public int IdProveedor { get; set; }
        
        [Required(ErrorMessage = "El nombre del archivo es obligatorio")]
        [StringLength(250, ErrorMessage = "El nombre del archivo no puede exceder 250 caracteres")]
        [Column(TypeName = "nvarchar(250)")]
        public string NombreArchivo { get; set; }
        
        [Required(ErrorMessage = "La ruta del archivo es obligatoria")]
        [Column(TypeName = "nvarchar(max)")]
        public string RutaArchivo { get; set; }

        // Navegación hacia el proveedor padre
        [ForeignKey("IdProveedor")]
        public virtual Proveedor Proveedor { get; set; }
    }
}
