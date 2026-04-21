using ApiVentas.Data;
using ApiVentas.Modelos;
using ApiVentas.Repositorio.IRepositorio;
using Microsoft.EntityFrameworkCore;

namespace ApiVentas.Repositorio
{
    public class UnidadMedidaRepositorio : IUnidadMedidaRepositorio
    {
        private readonly ApplicationDbContext _bd;

        public UnidadMedidaRepositorio(ApplicationDbContext bd)
        {
            _bd = bd;
        }

        public async Task<List<UnidadMedida>> ObtenerActivosAsync()
        {
            return await _bd.UnidadMedida
                .Where(u => u.EstadoRegistro)
                .OrderBy(u => u.IdUnidadMedida)
                .ToListAsync();
        }
    }
}
