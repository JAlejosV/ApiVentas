using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiVentas.Modelos
{
    public class Almacen
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdAlmacen { get; set; }
        
        [Required(ErrorMessage = "El código del almacén es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "El código del almacén debe ser un número válido mayor a 0")]
        public int CodigoAlmacen { get; set; }
        
        [Required(ErrorMessage = "El nombre del almacén es obligatorio")]
        [StringLength(150, ErrorMessage = "El nombre del almacén no puede exceder 150 caracteres")]
        public string NombreAlmacen { get; set; }
        
        [Required]
        public bool EstadoRegistro { get; set; } = true;
    }
}
