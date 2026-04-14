using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiVentas.Modelos
{
    public class PedidoArchivo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdPedidoArchivo { get; set; }

        [Required(ErrorMessage = "El pedido es obligatorio")]
        public int IdPedido { get; set; }

        [Required(ErrorMessage = "El nombre del archivo es obligatorio")]
        [StringLength(250, ErrorMessage = "El nombre del archivo no puede exceder 250 caracteres")]
        [Column(TypeName = "nvarchar(250)")]
        public string NombreArchivo { get; set; }

        [Required(ErrorMessage = "La ruta del archivo es obligatoria")]
        [Column(TypeName = "nvarchar(max)")]
        public string RutaArchivo { get; set; }

        [Required]
        [StringLength(100)]
        public string UsuarioCreacion { get; set; } = string.Empty;

        [Required]
        public DateTime FechaHoraCreacion { get; set; } = DateTime.Now;

        [StringLength(100)]
        public string UsuarioActualizacion { get; set; }

        public DateTime? FechaHoraActualizacion { get; set; }

        // Navegación hacia el pedido padre
        [ForeignKey("IdPedido")]
        public virtual Pedido Pedido { get; set; }
    }
}
