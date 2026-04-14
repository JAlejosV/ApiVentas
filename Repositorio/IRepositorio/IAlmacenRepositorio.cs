using ApiVentas.Modelos;

namespace ApiVentas.Repositorio.IRepositorio
{
    public interface IAlmacenRepositorio
    {
        ICollection<Almacen> GetAlmacenes();
        Almacen GetAlmacen(int idAlmacen);
        bool ExisteAlmacen(string nombreAlmacen);
        bool ExisteAlmacen(int idAlmacen);
        bool ExisteCodigoAlmacen(int codigoAlmacen);
        bool ExisteCodigoAlmacenExcluyendoId(int codigoAlmacen, int idAlmacenExcluir);
        bool ExisteAlmacenExcluyendoId(string nombreAlmacen, int idAlmacenExcluir);
        bool CrearAlmacen(Almacen almacen);
        bool ActualizarAlmacen(Almacen almacen);
        bool Guardar();
    }
}
