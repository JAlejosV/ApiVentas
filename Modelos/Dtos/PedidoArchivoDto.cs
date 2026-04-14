using System.ComponentModel.DataAnnotations;

namespace ApiVentas.Modelos.Dtos
{
    public class PedidoArchivoDto
    {
        public int IdPedidoArchivo { get; set; }
        public int IdPedido { get; set; }
        public string NombreArchivo { get; set; } = string.Empty;
        public string RutaArchivo { get; set; } = string.Empty;
        public string UsuarioCreacion { get; set; } = string.Empty;
        public DateTime FechaHoraCreacion { get; set; }
        public string UsuarioActualizacion { get; set; }
        public DateTime? FechaHoraActualizacion { get; set; }
    }

    public class PedidoArchivoCrearDto
    {
        [Required(ErrorMessage = "El pedido es obligatorio")]
        public int IdPedido { get; set; }

        [Required(ErrorMessage = "Los archivos son obligatorios")]
        public IFormFile[] Archivos { get; set; } = Array.Empty<IFormFile>();
    }
}
