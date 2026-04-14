using ApiVentas.Modelos;

namespace ApiVentas.Repositorio.IRepositorio
{
    public interface IEstadoRepositorio
    {
        ICollection<Estado> GetEstados();
        Estado GetEstado(int idEstado);
        bool ExisteEstado(string nombreEstado);
        bool ExisteEstado(int idEstado);
        bool ExisteCodigoEstado(string codigoEstado);
        bool ExisteCodigoEstadoExcluyendoId(string codigoEstado, int idEstadoExcluir);
        bool ExisteEstadoExcluyendoId(string nombreEstado, int idEstadoExcluir);
        bool CrearEstado(Estado estado);
        bool ActualizarEstado(Estado estado);
        bool Guardar();
    }
}
