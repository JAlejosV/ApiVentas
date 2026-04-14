using ApiVentas.Data;
using ApiVentas.Modelos;
using ApiVentas.Modelos.Dtos;
using ApiVentas.Repositorio.IRepositorio;
using Microsoft.EntityFrameworkCore;

namespace ApiVentas.Repositorio
{
    public class PedidoDetalleAlmacenRepositorio : IPedidoDetalleAlmacenRepositorio
    {
        private readonly ApplicationDbContext _bd;

        public PedidoDetalleAlmacenRepositorio(ApplicationDbContext bd)
        {
            _bd = bd;
        }

        public async Task<ICollection<PedidoDetalleAlmacenDto>> GetDistribucionPorDetalle(int idPedidoDetalle)
        {
            var distribuciones = await _bd.PedidoDetalleAlmacen
                .Include(pda => pda.Almacen)
                .Where(pda => pda.IdPedidoDetalle == idPedidoDetalle)
                .Select(pda => new PedidoDetalleAlmacenDto
                {
                    IdPedidoDetalleAlmacen = pda.IdPedidoDetalleAlmacen,
                    IdPedidoDetalle = pda.IdPedidoDetalle,
                    IdAlmacen = pda.IdAlmacen,
                    Cantidad = pda.Cantidad,
                    NombreAlmacen = pda.Almacen.NombreAlmacen
                })
                .ToListAsync();

            return distribuciones;
        }

        public async Task<bool> CrearDistribucion(List<PedidoDetalleAlmacen> distribuciones)
        {
            try
            {
                await _bd.PedidoDetalleAlmacen.AddRangeAsync(distribuciones);
                return await Guardar();
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> ActualizarDistribucion(int idPedidoDetalle, List<PedidoDetalleAlmacen> nuevasDistribuciones)
        {
            try
            {
                // Eliminar distribuciones existentes
                var distribucionesExistentes = await _bd.PedidoDetalleAlmacen
                    .Where(pda => pda.IdPedidoDetalle == idPedidoDetalle)
                    .ToListAsync();

                _bd.PedidoDetalleAlmacen.RemoveRange(distribucionesExistentes);

                // Agregar nuevas distribuciones
                await _bd.PedidoDetalleAlmacen.AddRangeAsync(nuevasDistribuciones);

                return await Guardar();
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> EliminarDistribucionDetalle(int idPedidoDetalle)
        {
            try
            {
                var distribuciones = await _bd.PedidoDetalleAlmacen
                    .Where(pda => pda.IdPedidoDetalle == idPedidoDetalle)
                    .ToListAsync();

                _bd.PedidoDetalleAlmacen.RemoveRange(distribuciones);
                return await Guardar();
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> ValidarDistribucion(int idPedidoDetalle, List<PedidoDetalleAlmacenCrearDto> distribuciones)
        {
            try
            {
                // Obtener la cantidad total del pedido detalle
                var pedidoDetalle = await _bd.PedidoDetalle
                    .FirstOrDefaultAsync(pd => pd.IdPedidoDetalle == idPedidoDetalle);

                if (pedidoDetalle == null)
                    return false;

                // Verificar que la suma de las distribuciones coincida con la cantidad total
                var cantidadDistribuida = distribuciones.Sum(d => d.Cantidad);
                return cantidadDistribuida == pedidoDetalle.CantidadPorPaquete;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<int> GetCantidadTotalDistribuida(int idPedidoDetalle)
        {
            try
            {
                var total = await _bd.PedidoDetalleAlmacen
                    .Where(pda => pda.IdPedidoDetalle == idPedidoDetalle)
                    .SumAsync(pda => pda.Cantidad);

                return total;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public async Task<bool> Guardar()
        {
            return await _bd.SaveChangesAsync() >= 0;
        }
    }
}
