using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiVentas.Modelos
{
    public class Estado
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdEstado { get; set; }
        
        [Required(ErrorMessage = "El código del estado es obligatorio")]
        [StringLength(2, ErrorMessage = "El código del estado no puede exceder 2 caracteres")]
        public string CodigoEstado { get; set; }
        
        [Required(ErrorMessage = "El nombre del estado es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre del estado no puede exceder 100 caracteres")]
        public string NombreEstado { get; set; }
        
        [Required(ErrorMessage = "La descripción del estado es obligatoria")]
        [StringLength(250, ErrorMessage = "La descripción del estado no puede exceder 250 caracteres")]
        public string DescripcionEstado { get; set; }
        
        [Required]
        public bool EstadoRegistro { get; set; } = true;
    }
}
