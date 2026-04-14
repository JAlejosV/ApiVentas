using System.ComponentModel.DataAnnotations;

namespace ApiVentas.Modelos.Dtos
{
    public class AutenticacionRegistroDto
    {
        [Required(ErrorMessage = "El nombre completo es obligatorio")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 200 caracteres")]
        public string NombreCompleto { get; set; }

        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "Debe ser un correo válido")]
        [StringLength(200, ErrorMessage = "El correo no puede exceder 200 caracteres")]
        public string Correo { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        [MaxLength(100, ErrorMessage = "La contraseña no puede exceder 100 caracteres")]
        public string Password { get; set; }

        [Required(ErrorMessage = "La confirmación de contraseña es obligatoria")]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Debe especificar al menos un rol")]
        public List<int> RolesIds { get; set; } = new List<int>();
    }
}