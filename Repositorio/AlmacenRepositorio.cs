using ApiVentas.Data;
using ApiVentas.Modelos;
using ApiVentas.Repositorio.IRepositorio;
using Microsoft.EntityFrameworkCore;

namespace ApiVentas.Repositorio
{
    public class AlmacenRepositorio : IAlmacenRepositorio
    {
        private readonly ApplicationDbContext _bd;

        public AlmacenRepositorio(ApplicationDbContext bd)
        {
            _bd = bd;
        }

        public bool ActualizarAlmacen(Almacen almacen)
        {
            try
            {
                // Buscar la entidad existente en el contexto
                var almacenExistente = _bd.Almacen.Find(almacen.IdAlmacen);
                
                if (almacenExistente == null)
                    return false;

                // Actualizar solo los campos modificables
                almacenExistente.CodigoAlmacen = almacen.CodigoAlmacen;
                almacenExistente.NombreAlmacen = almacen.NombreAlmacen.Trim();
                almacenExistente.EstadoRegistro = almacen.EstadoRegistro;

                return Guardar();
            }
            catch
            {
                return false;
            }
        }

        public bool CrearAlmacen(Almacen almacen)
        {
            almacen.NombreAlmacen = almacen.NombreAlmacen.Trim();
            _bd.Almacen.Add(almacen);
            return Guardar();
        }

        public bool ExisteAlmacen(string nombreAlmacen)
        {
            bool valor = _bd.Almacen.Any(a => a.NombreAlmacen.ToLower().Trim() == nombreAlmacen.ToLower().Trim());
            return valor;
        }

        public bool ExisteAlmacen(int idAlmacen)
        {
            return _bd.Almacen.Any(a => a.IdAlmacen == idAlmacen);
        }

        public bool ExisteCodigoAlmacen(int codigoAlmacen)
        {
            bool valor = _bd.Almacen.Any(a => a.CodigoAlmacen == codigoAlmacen);
            return valor;
        }

        public bool ExisteCodigoAlmacenExcluyendoId(int codigoAlmacen, int idAlmacenExcluir)
        {
            return _bd.Almacen.Any(a => a.CodigoAlmacen == codigoAlmacen && a.IdAlmacen != idAlmacenExcluir);
        }

        public bool ExisteAlmacenExcluyendoId(string nombreAlmacen, int idAlmacenExcluir)
        {
            return _bd.Almacen.Any(a => a.NombreAlmacen.ToLower().Trim() == nombreAlmacen.ToLower().Trim() && a.IdAlmacen != idAlmacenExcluir);
        }

        public Almacen GetAlmacen(int idAlmacen)
        {
            return _bd.Almacen.FirstOrDefault(a => a.IdAlmacen == idAlmacen);
        }

        public ICollection<Almacen> GetAlmacenes()
        {
            return _bd.Almacen.OrderBy(a => a.NombreAlmacen).ToList();
        }

        public bool Guardar()
        {
            return _bd.SaveChanges() >= 0 ? true : false;
        }
    }
}
