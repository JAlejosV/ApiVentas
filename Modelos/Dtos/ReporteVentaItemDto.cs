namespace ApiVentas.Modelos.Dtos
{
    public class ReporteVentaItemDto
    {
        public string FechaEmision { get; set; }
        public string TipoDocumento { get; set; }
        public string Serie { get; set; }
        public string NumeroDocumento { get; set; }
        public string CodigoProducto { get; set; }
        public string Producto { get; set; }
        public string UnidadMedida { get; set; }
        public decimal CantidadVenta { get; set; }
        public decimal UnidadesPorPresentacion { get; set; }
        public string Moneda { get; set; }
        public decimal PrecioCompra { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal CantidadPorUnidades { get; set; }
        public decimal Ganancia { get; set; }
        public decimal Total { get; set; }
        public int IdVendedor { get; set; }
        public string Vendedor { get; set; }
    }
}
