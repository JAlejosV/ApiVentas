using ApiVentas.Modelos;

namespace ApiVentas.Repositorio.IRepositorio
{
    public interface IProveedorArchivoRepositorio
    {
        ICollection<ProveedorArchivo> GetArchivosByProveedor(int idProveedor);
        ProveedorArchivo GetArchivo(int idArchivo);
        bool CrearArchivo(ProveedorArchivo archivo);
        bool ActualizarArchivo(ProveedorArchivo archivo);
        bool BorrarArchivo(ProveedorArchivo archivo);
        bool ExisteArchivo(int idArchivo);
        bool Guardar();
    }
}
