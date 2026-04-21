using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiVentas.Modelos
{
    public class UnidadMedida
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdUnidadMedida { get; set; }

        [Required(ErrorMessage = "El código de unidad de medida es obligatorio")]
        [StringLength(10, ErrorMessage = "El código no puede exceder 10 caracteres")]
        public string CodigoUnidadMedida { get; set; }

        [Required(ErrorMessage = "El nombre de unidad de medida es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string NombreUnidadMedida { get; set; }

        [Required]
        public bool EstadoRegistro { get; set; } = true;
    }
}
