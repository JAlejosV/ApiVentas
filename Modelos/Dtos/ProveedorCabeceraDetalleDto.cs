using Microsoft.AspNetCore.Http;

namespace ApiVentas.Modelos.Dtos
{
    public class ProveedorCabeceraDetalleDto
    {
        public int IdProveedor { get; set; }
        public int CodigoProveedor { get; set; }
        public string NombreProveedor { get; set; }
        public string Telefono { get; set; }
        public string Contacto { get; set; }
        public bool EstadoRegistro { get; set; }
        
        // Archivos existentes
        public List<ProveedorArchivoDetalleDto> ArchivosExistentes { get; set; } = new List<ProveedorArchivoDetalleDto>();
        
        // Para eliminar archivos (IDs)
        public List<int> ArchivosAEliminar { get; set; } = new List<int>();
        
        // Para subir nuevos archivos
        public List<IFormFile> ArchivosNuevos { get; set; } = new List<IFormFile>();
    }
}
