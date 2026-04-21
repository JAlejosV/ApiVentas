using ApiVentas.Modelos;

namespace ApiVentas.Repositorio.IRepositorio
{
    public interface IUnidadMedidaRepositorio
    {
        Task<List<UnidadMedida>> ObtenerActivosAsync();
    }
}
