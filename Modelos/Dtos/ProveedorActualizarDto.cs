using System.ComponentModel.DataAnnotations;

namespace ApiVentas.Modelos.Dtos
{
    public class ProveedorActualizarDto
    {
        [Required(ErrorMessage = "El ID del proveedor es obligatorio")]
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
        
        public bool EstadoRegistro { get; set; }
    }
}
