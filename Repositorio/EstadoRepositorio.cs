using ApiVentas.Data;
using ApiVentas.Modelos;
using ApiVentas.Repositorio.IRepositorio;
using Microsoft.EntityFrameworkCore;

namespace ApiVentas.Repositorio
{
    public class EstadoRepositorio : IEstadoRepositorio
    {
        private readonly ApplicationDbContext _bd;

        public EstadoRepositorio(ApplicationDbContext bd)
        {
            _bd = bd;
        }

        public bool ActualizarEstado(Estado estado)
        {
            try
            {
                // Buscar la entidad existente en el contexto
                var estadoExistente = _bd.Estado.Find(estado.IdEstado);
                
                if (estadoExistente == null)
                    return false;

                // Actualizar solo los campos modificables
                estadoExistente.CodigoEstado = estado.CodigoEstado.Trim().ToUpper();
                estadoExistente.NombreEstado = estado.NombreEstado.Trim();
                estadoExistente.DescripcionEstado = estado.DescripcionEstado.Trim();
                estadoExistente.EstadoRegistro = estado.EstadoRegistro;

                return Guardar();
            }
            catch
            {
                return false;
            }
        }

        public bool CrearEstado(Estado estado)
        {
            estado.NombreEstado = estado.NombreEstado.Trim();
            estado.DescripcionEstado = estado.DescripcionEstado.Trim();
            estado.CodigoEstado = estado.CodigoEstado.Trim().ToUpper();
            _bd.Estado.Add(estado);
            return Guardar();
        }

        public bool ExisteEstado(string nombreEstado)
        {
            bool valor = _bd.Estado.Any(e => e.NombreEstado.ToLower().Trim() == nombreEstado.ToLower().Trim());
            return valor;
        }

        public bool ExisteEstado(int idEstado)
        {
            return _bd.Estado.Any(e => e.IdEstado == idEstado);
        }

        public bool ExisteCodigoEstado(string codigoEstado)
        {
            bool valor = _bd.Estado.Any(e => e.CodigoEstado.ToLower().Trim() == codigoEstado.ToLower().Trim());
            return valor;
        }

        public bool ExisteCodigoEstadoExcluyendoId(string codigoEstado, int idEstadoExcluir)
        {
            return _bd.Estado.Any(e => e.CodigoEstado.ToLower().Trim() == codigoEstado.ToLower().Trim() && e.IdEstado != idEstadoExcluir);
        }

        public bool ExisteEstadoExcluyendoId(string nombreEstado, int idEstadoExcluir)
        {
            return _bd.Estado.Any(e => e.NombreEstado.ToLower().Trim() == nombreEstado.ToLower().Trim() && e.IdEstado != idEstadoExcluir);
        }

        public Estado GetEstado(int idEstado)
        {
            return _bd.Estado.FirstOrDefault(e => e.IdEstado == idEstado);
        }

        public ICollection<Estado> GetEstados()
        {
            return _bd.Estado.OrderBy(e => e.NombreEstado).ToList();
        }

        public bool Guardar()
        {
            return _bd.SaveChanges() >= 0 ? true : false;
        }
    }
}
