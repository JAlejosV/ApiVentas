using System.ComponentModel.DataAnnotations;

namespace ApiVentas.Modelos.Dtos
{
    public class PedidoDetalleAlmacenDto
    {
        public int IdPedidoDetalleAlmacen { get; set; }
        public int IdPedidoDetalle { get; set; }
        public int IdAlmacen { get; set; }
        public string NombreAlmacen { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public string UsuarioCreacion { get; set; } = string.Empty;
        public DateTime FechaHoraCreacion { get; set; }
        public string UsuarioActualizacion { get; set; }
        public DateTime? FechaHoraActualizacion { get; set; }
    }
}
