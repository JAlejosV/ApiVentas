using System.ComponentModel.DataAnnotations;

namespace ApiVentas.Modelos.Dtos
{
    public class EstadoDto
    {
        public int IdEstado { get; set; }
        public string CodigoEstado { get; set; }
        public string NombreEstado { get; set; }
        public string DescripcionEstado { get; set; }
        public bool EstadoRegistro { get; set; }
    }
}
