using System.ComponentModel.DataAnnotations;

namespace ApiVentas.Modelos.Dtos
{
    public class ProductoDto
    {
        public int IdProducto { get; set; }
        public string CodigoProducto { get; set; }
        public string NombreProducto { get; set; }
        public int? StockReal { get; set; }
        public int? StockMinimo { get; set; }
        public bool EstadoRegistro { get; set; }
        
        // Lista de proveedores con sus precios
        public List<ProductoProveedorDto> Proveedores { get; set; } = new List<ProductoProveedorDto>();
        
        // String formateado para mostrar en la tabla (Lima (S/24.00), Arequipa (S/23.50))
        public string ProveedoresFormateado { get; set; }
    }
    
    public class ProductoProveedorDto
    {
        public int IdProductoProveedor { get; set; }
        public int IdProveedor { get; set; }
        public int IdProducto { get; set; }
        public string NombreProveedor { get; set; }
        public int CantidadPorPaquete { get; set; }
        public decimal PrecioPorPaquete { get; set; }
        public decimal PrecioUnitario { get; set; }
        public bool EstadoRegistro { get; set; }
    }
}
