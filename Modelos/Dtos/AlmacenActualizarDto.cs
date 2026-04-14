using System.ComponentModel.DataAnnotations;

namespace ApiVentas.Modelos.Dtos
{
    public class AlmacenActualizarDto
    {
        [Required(ErrorMessage = "El ID del almacén es obligatorio")]
        public int IdAlmacen { get; set; }
        
        [Required(ErrorMessage = "El código del almacén es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "El código del almacén debe ser un número válido mayor a 0")]
        public int CodigoAlmacen { get; set; }
        
        [Required(ErrorMessage = "El nombre del almacén es obligatorio")]
        [StringLength(150, ErrorMessage = "El nombre del almacén no puede exceder 150 caracteres")]
        public string NombreAlmacen { get; set; }
        
        public bool EstadoRegistro { get; set; }
    }
}
