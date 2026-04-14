using System.ComponentModel.DataAnnotations;

namespace ApiVentas.Modelos.Dtos
{
    /// <summary>
    /// DTO para cambiar el estado de registro de entidades
    /// </summary>
    public class EstadoRegistroDto
    {
        /// <summary>
        /// Nuevo estado de registro (true = activo, false = inactivo)
        /// </summary>
        [Required(ErrorMessage = "El estado de registro es obligatorio")]
        public bool EstadoRegistro { get; set; }
    }
}