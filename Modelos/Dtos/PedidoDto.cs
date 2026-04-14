using System.ComponentModel.DataAnnotations;

namespace ApiVentas.Modelos.Dtos
{
    public class PedidoDto
    {
        public int IdPedido { get; set; }
        public int IdProveedor { get; set; }
        public string NombreProveedor { get; set; } = string.Empty;
        public int IdEstado { get; set; }
        public string NombreEstado { get; set; } = string.Empty;
        public string CodigoEstado { get; set; } = string.Empty;
        public decimal MontoTotal { get; set; }
        public string UsuarioCreacion { get; set; } = string.Empty;
        public DateTime FechaHoraCreacion { get; set; }
        public string UsuarioActualizacion { get; set; }
        public DateTime? FechaHoraActualizacion { get; set; }
        public List<PedidoDetalleDto> PedidoDetalles { get; set; } = new List<PedidoDetalleDto>();
        public List<PedidoArchivoDto> PedidoArchivos { get; set; } = new List<PedidoArchivoDto>();
    }
}
