using ApiVentas.Data;
using ApiVentas.Modelos;
using ApiVentas.Repositorio.IRepositorio;

namespace ApiVentas.Repositorio
{
    public class ProveedorArchivoRepositorio : IProveedorArchivoRepositorio
    {
        private readonly ApplicationDbContext _db;

        public ProveedorArchivoRepositorio(ApplicationDbContext db)
        {
            _db = db;
        }

        public bool ActualizarArchivo(ProveedorArchivo archivo)
        {
            _db.ProveedorArchivo.Update(archivo);
            return Guardar();
        }

        public bool BorrarArchivo(ProveedorArchivo archivo)
        {
            _db.ProveedorArchivo.Remove(archivo);
            return Guardar();
        }

        public bool CrearArchivo(ProveedorArchivo archivo)
        {
            _db.ProveedorArchivo.Add(archivo);
            return Guardar();
        }

        public bool ExisteArchivo(int idArchivo)
        {
            return _db.ProveedorArchivo.Any(x => x.IdProveedorArchivo == idArchivo);
        }

        public ProveedorArchivo GetArchivo(int idArchivo)
        {
            return _db.ProveedorArchivo.FirstOrDefault(x => x.IdProveedorArchivo == idArchivo);
        }

        public ICollection<ProveedorArchivo> GetArchivosByProveedor(int idProveedor)
        {
            return _db.ProveedorArchivo.Where(x => x.IdProveedor == idProveedor).OrderBy(x => x.NombreArchivo).ToList();
        }

        public bool Guardar()
        {
            return _db.SaveChanges() >= 0;
        }
    }
}
