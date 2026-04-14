namespace ApiVentas.Modelos.Dtos
{
    public class TokenValidacionDto
    {
        public bool EsValido { get; set; }
        public int UsuarioId { get; set; }
        public string Correo { get; set; }
        public DateTime ExpiraEn { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
        public List<string> Permisos { get; set; } = new List<string>();
    }
}