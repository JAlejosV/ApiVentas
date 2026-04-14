namespace ApiVentas.Modelos.Dtos
{
    public class AutenticacionRespuestaDto
    {
        public string Token { get; set; }
        public string TipoToken { get; set; } = "Bearer";
        public int ExpiraEn { get; set; }
        public UsuarioInfoDto Usuario { get; set; }
        public DateTime FechaExpiracion { get; set; }
    }
}