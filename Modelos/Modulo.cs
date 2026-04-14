using System.ComponentModel.DataAnnotations;

namespace ApiVentas.Modelos
{
    public class Modulo
    {
        [Key]
        public int IdModulo { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Nombre { get; set; }
        
        public string Descripcion { get; set; }
        
        // Navegación
        public virtual ICollection<Permiso> Permisos { get; set; } = new List<Permiso>();
    }
}