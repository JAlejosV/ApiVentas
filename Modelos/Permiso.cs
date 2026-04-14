using System.ComponentModel.DataAnnotations;

namespace ApiVentas.Modelos
{
    public class Permiso
    {
        [Key]
        public int IdPermiso { get; set; }
        
        [Required]
        public int IdModulo { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Nombre { get; set; }
        
        public string Descripcion { get; set; }
        
        // Navegación
        public virtual Modulo Modulo { get; set; }
        public virtual ICollection<RolPermiso> RolPermisos { get; set; } = new List<RolPermiso>();
    }
}