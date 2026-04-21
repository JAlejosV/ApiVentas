using System.ComponentModel.DataAnnotations;

namespace ApiVentas.Modelos
{
    public class UsuarioSistema
    {
        [Key]
        public int IdUsuario { get; set; }
        
        public int? CodigoEquivalencia { get; set; }
        
        [Required]
        [StringLength(200)]
        public string NombreCompleto { get; set; }
        
        [Required]
        [StringLength(200)]
        [EmailAddress]
        public string Correo { get; set; }
        
        [Required]
        public string PasswordHash { get; set; }
        
        public bool EstadoRegistro { get; set; } = true;
        
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        
        // Navegación
        public virtual ICollection<UsuarioRol> UsuarioRoles { get; set; } = new List<UsuarioRol>();
    }
}