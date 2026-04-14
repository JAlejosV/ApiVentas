using System.ComponentModel.DataAnnotations;

namespace ApiVentas.Modelos
{
    public class UsuarioRol
    {
        public int IdUsuario { get; set; }
        public int IdRol { get; set; }
        public DateTime FechaAsignacion { get; set; } = DateTime.UtcNow;
        
        // Navegación
        public virtual UsuarioSistema Usuario { get; set; }
        public virtual Rol Rol { get; set; }
    }
}