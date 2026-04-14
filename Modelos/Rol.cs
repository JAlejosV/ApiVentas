using System.ComponentModel.DataAnnotations;

namespace ApiVentas.Modelos
{
    public class Rol
    {
        [Key]
        public int IdRol { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Nombre { get; set; }
        
        public string Descripcion { get; set; }
        
        // Navegación
        public virtual ICollection<UsuarioRol> UsuarioRoles { get; set; } = new List<UsuarioRol>();
        public virtual ICollection<RolPermiso> RolPermisos { get; set; } = new List<RolPermiso>();
    }
}