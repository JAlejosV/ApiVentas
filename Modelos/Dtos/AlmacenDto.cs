using System.ComponentModel.DataAnnotations;

namespace ApiVentas.Modelos.Dtos
{
    public class AlmacenDto
    {
        public int IdAlmacen { get; set; }
        public int CodigoAlmacen { get; set; }
        public string NombreAlmacen { get; set; }
        public bool EstadoRegistro { get; set; }
    }
}
