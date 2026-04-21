namespace ApiVentas.Modelos.Dtos
{
    public class UsuarioListaDto
    {
        public int IdUsuario { get; set; }
        public int? CodigoEquivalencia { get; set; }
        public string NombreCompleto { get; set; }
        public string Correo { get; set; }
        public bool EstadoRegistro { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}
