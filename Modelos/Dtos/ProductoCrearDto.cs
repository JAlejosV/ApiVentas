using System.ComponentModel.DataAnnotations;

namespace ApiVentas.Modelos.Dtos
{
    public class ProductoCrearDto
    {
        [Required(ErrorMessage = "El código del producto es obligatorio")]
        [StringLength(30, ErrorMessage = "El código no puede exceder 30 caracteres")]
        public string CodigoProducto { get; set; }

        [Required(ErrorMessage = "El nombre del producto es obligatorio")]
        [StringLength(600, ErrorMessage = "El nombre no puede exceder 600 caracteres")]
        public string NombreProducto { get; set; }

        public int? StockReal { get; set; }

        public int? StockMinimo { get; set; }

        [Required(ErrorMessage = "Debe agregar al menos un proveedor")]
        public List<ProductoProveedorCrearDto> Proveedores { get; set; } = new List<ProductoProveedorCrearDto>();
    }

    public class ProductoProveedorCrearDto
    {
        [Required(ErrorMessage = "El proveedor es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un proveedor válido")]
        public int IdProveedor { get; set; }

        [Required(ErrorMessage = "La cantidad por paquete es obligatoria")]
        [Range(0, int.MaxValue, ErrorMessage = "La cantidad por paquete debe ser mayor o igual a 0")]
        public int CantidadPorPaquete { get; set; }

        [Required(ErrorMessage = "El precio por paquete es obligatorio")]
        [Range(0.000001, double.MaxValue, ErrorMessage = "El precio por paquete debe ser mayor a 0")]
        public decimal PrecioPorPaquete { get; set; }

        [Required(ErrorMessage = "El precio unitario es obligatorio")]
        [Range(0.000001, double.MaxValue, ErrorMessage = "El precio unitario debe ser mayor a 0")]
        public decimal PrecioUnitario { get; set; }

        [Required]
        public bool EstadoRegistro { get; set; } = true;
    }
}
