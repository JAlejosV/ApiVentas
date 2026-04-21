using ApiVentas.Modelos.Dtos;

namespace ApiVentas.Repositorio.IRepositorio
{
    public interface IReporteRepositorio
    {
        Task<List<ReporteVentaItemDto>> ObtenerReporteVentasAsync(
            string fechaInicio,
            string fechaFin,
            int? idUsuario,
            string codigoProducto,
            string unidadMedida);
    }
}
