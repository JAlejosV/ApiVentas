using System.ComponentModel.DataAnnotations;

namespace ApiVentas.Modelos.Dtos
{
    public class EstadoActualizarDto
    {
        [Required(ErrorMessage = "El ID del estado es obligatorio")]
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
        
        public bool EstadoRegistro { get; set; }
    }
}
