namespace ApiVentas.Modelos.Dtos
{
    public class UsuarioInfoDto
    {
        public int Id { get; set; }
        public string NombreCompleto { get; set; }
        public string Correo { get; set; }
        public bool EstadoRegistro { get; set; }
        public DateTime FechaCreacion { get; set; }
        public List<RolDto> Roles { get; set; } = new List<RolDto>();
        public List<PermisoDto> Permisos { get; set; } = new List<PermisoDto>();
    }
}