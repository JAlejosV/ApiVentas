using System.ComponentModel.DataAnnotations;

namespace ApiVentas.Modelos.Dtos
{
    public class ProveedorDto
    {
        public int IdProveedor { get; set; }
        public int CodigoProveedor { get; set; }
        public string NombreProveedor { get; set; }
        public string Telefono { get; set; }
        public string Contacto { get; set; }
        public bool EstadoRegistro { get; set; }
    }
}
