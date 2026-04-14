namespace ApiVentas.Modelos.Dtos
{
    public class PedidoFiltroDto
    {
        public int? IdProveedor { get; set; }
        public string NombreProveedor { get; set; }
        public int? IdEstado { get; set; }
        public DateTime FechaInicio { get; set; } = DateTime.Now.AddDays(-7);
        public DateTime FechaFin { get; set; } = DateTime.Now;
    }
}
