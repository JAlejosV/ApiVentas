using System.ComponentModel.DataAnnotations;

namespace ApiVentas.Modelos.Dtos
{
    public class ProductoProveedorPreciosDto
    {
        [Required]
        public int IdProductoProveedor { get; set; }
        
        [Required(ErrorMessage = "La cantidad por paquete es obligatoria")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad por paquete debe ser mayor a 0")]
        public int CantidadPorPaquete { get; set; }
        
        [Required(ErrorMessage = "El precio por paquete es obligatorio")]
        [Range(0, double.MaxValue, ErrorMessage = "El precio por paquete debe ser mayor o igual a 0")]
        public decimal PrecioPorPaquete { get; set; }
        
        [Range(0, double.MaxValue, ErrorMessage = "El precio unitario debe ser mayor o igual a 0")]
        public decimal PrecioUnitario { get; set; }
    }
}
