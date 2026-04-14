using System.ComponentModel.DataAnnotations;

namespace ApiVentas.Modelos
{
    public class RolPermiso
    {
        public int IdRol { get; set; }
        public int IdPermiso { get; set; }
        
        // Navegación
        public virtual Rol Rol { get; set; }
        public virtual Permiso Permiso { get; set; }
    }
}