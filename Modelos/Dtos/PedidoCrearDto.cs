using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ApiVentas.Modelos.Dtos
{
    public class PedidoCrearDto
    {
        [Required(ErrorMessage = "El proveedor es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un proveedor válido")]
        public int IdProveedor { get; set; }

        [Required(ErrorMessage = "El estado es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un estado válido")]
        public int IdEstado { get; set; }

        [Required(ErrorMessage = "El monto total es obligatorio")]
        [Range(0, double.MaxValue, ErrorMessage = "El monto total debe ser mayor o igual a 0")]
        public decimal MontoTotal { get; set; }

        [Required(ErrorMessage = "Debe agregar al menos un detalle")]
        public List<PedidoDetalleCrearDto> Detalles { get; set; } = new List<PedidoDetalleCrearDto>();

        // Para subir archivos
        public List<IFormFile> Archivos { get; set; } = new List<IFormFile>();
    }
}
