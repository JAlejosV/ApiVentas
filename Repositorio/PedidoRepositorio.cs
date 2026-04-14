using ApiVentas.Data;
using ApiVentas.Modelos;
using ApiVentas.Modelos.Dtos;
using ApiVentas.Repositorio.IRepositorio;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace ApiVentas.Repositorio
{
    public class PedidoRepositorio : IPedidoRepositorio
    {
        private readonly ApplicationDbContext _bd;
        private readonly IMapper _mapper;

        public PedidoRepositorio(ApplicationDbContext bd, IMapper mapper)
        {
            _bd = bd;
            _mapper = mapper;
        }

        public async Task<ICollection<PedidoDto>> GetPedidos()
        {
            var listaPedidos = await _bd.Pedido
                .Include(p => p.Proveedor)
                .Include(p => p.Estado)
                .Include(p => p.PedidoDetalles)
                    .ThenInclude(pd => pd.ProductoProveedor)
                        .ThenInclude(pp => pp.Producto)
                .Include(p => p.PedidoDetalles)
                    .ThenInclude(pd => pd.PedidoDetalleAlmacenes)
                        .ThenInclude(pda => pda.Almacen)
                .Include(p => p.PedidoArchivos)
                .OrderByDescending(p => p.FechaHoraCreacion)
                .ToListAsync();

            return _mapper.Map<ICollection<PedidoDto>>(listaPedidos);
        }

        public async Task<ICollection<PedidoDto>> BuscarPedidos(PedidoFiltroDto filtro)
        {
            var query = _bd.Pedido
                .Include(p => p.Proveedor)
                .Include(p => p.Estado)
                .Include(p => p.PedidoDetalles)
                    .ThenInclude(pd => pd.ProductoProveedor)
                        .ThenInclude(pp => pp.Producto)
                .Include(p => p.PedidoDetalles)
                    .ThenInclude(pd => pd.PedidoDetalleAlmacenes)
                        .ThenInclude(pda => pda.Almacen)
                .Include(p => p.PedidoArchivos)
                .AsQueryable();

            // Filtro por proveedor
            if (filtro.IdProveedor.HasValue && filtro.IdProveedor > 0)
            {
                query = query.Where(p => p.IdProveedor == filtro.IdProveedor.Value);
            }
            else if (!string.IsNullOrEmpty(filtro.NombreProveedor))
            {
                query = query.Where(p => p.Proveedor.NombreProveedor.ToLower().Contains(filtro.NombreProveedor.ToLower()));
            }

            // Filtro por estado
            if (filtro.IdEstado.HasValue && filtro.IdEstado > 0)
            {
                query = query.Where(p => p.IdEstado == filtro.IdEstado.Value);
            }

            // Filtro por fechas
            query = query.Where(p => p.FechaHoraCreacion.Date >= filtro.FechaInicio.Date &&
                                   p.FechaHoraCreacion.Date <= filtro.FechaFin.Date);

            var listaPedidos = await query
                .OrderByDescending(p => p.FechaHoraCreacion)
                .ToListAsync();

            return _mapper.Map<ICollection<PedidoDto>>(listaPedidos);
        }

        public async Task<PedidoDto> GetPedido(int id)
        {
            var pedido = await _bd.Pedido
                .Include(p => p.Proveedor)
                .Include(p => p.Estado)
                .Include(p => p.PedidoDetalles)
                    .ThenInclude(pd => pd.ProductoProveedor)
                        .ThenInclude(pp => pp.Producto)
                .Include(p => p.PedidoDetalles)
                    .ThenInclude(pd => pd.ProductoProveedor)
                        .ThenInclude(pp => pp.Proveedor)
                .Include(p => p.PedidoDetalles)
                    .ThenInclude(pd => pd.PedidoDetalleAlmacenes)
                        .ThenInclude(pda => pda.Almacen)
                .Include(p => p.PedidoArchivos)
                .FirstOrDefaultAsync(p => p.IdPedido == id);

            return _mapper.Map<PedidoDto>(pedido);
        }

        public async Task<bool> ExistePedido(int id)
        {
            return await _bd.Pedido.AnyAsync(p => p.IdPedido == id);
        }

        public async Task<bool> CrearPedido(Pedido pedido)
        {
            await _bd.Pedido.AddAsync(pedido);
            return await Guardar();
        }

        public async Task<bool> ActualizarPedido(Pedido pedido)
        {
            var pedidoExistente = await _bd.Pedido
                .Include(p => p.PedidoDetalles)
                    .ThenInclude(pd => pd.PedidoDetalleAlmacenes)
                .FirstOrDefaultAsync(p => p.IdPedido == pedido.IdPedido);

            if (pedidoExistente == null)
                return false;

            // Actualizar propiedades principales
            pedidoExistente.IdProveedor = pedido.IdProveedor;
            pedidoExistente.IdEstado = pedido.IdEstado;
            pedidoExistente.MontoTotal = pedido.MontoTotal;
            pedidoExistente.UsuarioActualizacion = pedido.UsuarioActualizacion;
            pedidoExistente.FechaHoraActualizacion = DateTime.Now;

            // Eliminar detalles existentes
            _bd.PedidoDetalleAlmacen.RemoveRange(
                pedidoExistente.PedidoDetalles.SelectMany(pd => pd.PedidoDetalleAlmacenes));
            _bd.PedidoDetalle.RemoveRange(pedidoExistente.PedidoDetalles);

            // Agregar nuevos detalles
            pedidoExistente.PedidoDetalles = pedido.PedidoDetalles;

            return await Guardar();
        }

        public async Task<bool> VerificarPedido(int id, string usuario, decimal? montoVerificacion = null)
        {
            var pedido = await _bd.Pedido.FindAsync(id);
            if (pedido == null) return false;

            // Validar monto si se proporciona
            if (montoVerificacion.HasValue)
            {
                var diferencia = Math.Abs(pedido.MontoTotal - montoVerificacion.Value);
                if (diferencia > 0.5m) // Margen de ±S/0.5
                {
                    return false;
                }
            }

            // Buscar el estado "Verificado" (VE)
            var estadoVerificado = await _bd.Estado.FirstOrDefaultAsync(e => e.CodigoEstado == "VE");
            if (estadoVerificado == null) return false;

            return await CambiarEstadoPedido(id, estadoVerificado.IdEstado, usuario);
        }

        public async Task<bool> TrasladarPedido(int id, string usuario)
        {
            // Buscar el estado "Trasladado" (TR)
            var estadoTrasladado = await _bd.Estado.FirstOrDefaultAsync(e => e.CodigoEstado == "TR");
            if (estadoTrasladado == null) return false;

            return await CambiarEstadoPedido(id, estadoTrasladado.IdEstado, usuario);
        }

        public async Task<bool> AnularPedido(int id, string usuario)
        {
            // Buscar el estado "Anulado" (AN)
            var estadoAnulado = await _bd.Estado.FirstOrDefaultAsync(e => e.CodigoEstado == "AN");
            if (estadoAnulado == null) return false;

            return await CambiarEstadoPedido(id, estadoAnulado.IdEstado, usuario);
        }

        public async Task<bool> ReactivarPedido(int id, string usuario)
        {
            // Buscar el estado "Registrado" (RE) para reactivar pedidos anulados
            var estadoRegistrado = await _bd.Estado.FirstOrDefaultAsync(e => e.CodigoEstado == "RE");
            if (estadoRegistrado == null) return false;

            return await CambiarEstadoPedido(id, estadoRegistrado.IdEstado, usuario);
        }

        public async Task<bool> CambiarEstadoPedido(int idPedido, int idEstado, string usuario)
        {
            var pedido = await _bd.Pedido.FindAsync(idPedido);
            if (pedido == null) return false;

            pedido.IdEstado = idEstado;
            pedido.UsuarioActualizacion = usuario;
            pedido.FechaHoraActualizacion = DateTime.Now;

            return await Guardar();
        }

        public async Task<bool> AgregarArchivo(PedidoArchivo archivo)
        {
            await _bd.PedidoArchivo.AddAsync(archivo);
            return await Guardar();
        }

        public async Task<bool> EliminarArchivo(int idArchivo)
        {
            var archivo = await _bd.PedidoArchivo.FindAsync(idArchivo);
            if (archivo == null) return false;

            _bd.PedidoArchivo.Remove(archivo);
            return await Guardar();
        }

        public async Task<ICollection<PedidoArchivoDto>> GetArchivosPedido(int idPedido)
        {
            var archivos = await _bd.PedidoArchivo
                .Where(pa => pa.IdPedido == idPedido)
                .ToListAsync();

            return _mapper.Map<ICollection<PedidoArchivoDto>>(archivos);
        }

        public async Task<PedidoArchivoDto> GetArchivo(int idArchivo)
        {
            var archivo = await _bd.PedidoArchivo.FindAsync(idArchivo);
            return archivo != null ? _mapper.Map<PedidoArchivoDto>(archivo) : null;
        }

        public async Task<bool> DistribuirPorAlmacenes(DistribucionAlmacenesDto distribucion, string usuario)
        {
            try
            {
                // Verificar que existe el detalle del pedido
                var pedidoDetalle = await _bd.PedidoDetalle
                    .Include(pd => pd.PedidoDetalleAlmacenes)
                    .FirstOrDefaultAsync(pd => pd.IdPedidoDetalle == distribucion.IdPedidoDetalle);

                if (pedidoDetalle == null)
                {
                    return false;
                }

                // Validar que la suma de las distribuciones no exceda la cantidad total
                var cantidadTotal = distribucion.Distribuciones.Sum(d => d.Cantidad);
                if (cantidadTotal != pedidoDetalle.TotalUnidades)
                {
                    return false; // La distribución debe coincidir exactamente con la cantidad del detalle
                }

                // Eliminar distribuciones existentes
                _bd.PedidoDetalleAlmacen.RemoveRange(pedidoDetalle.PedidoDetalleAlmacenes);

                // Crear nuevas distribuciones
                foreach (var dist in distribucion.Distribuciones)
                {
                    var pedidoDetalleAlmacen = new PedidoDetalleAlmacen
                    {
                        IdPedidoDetalle = distribucion.IdPedidoDetalle,
                        IdAlmacen = dist.IdAlmacen,
                        Cantidad = dist.Cantidad,
                        UsuarioCreacion = usuario,
                        FechaHoraCreacion = DateTime.UtcNow
                    };

                    await _bd.PedidoDetalleAlmacen.AddAsync(pedidoDetalleAlmacen);
                }

                return await Guardar();
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ValidarMontoVerificacion(int idPedido, decimal montoVerificacion)
        {
            var pedido = await _bd.Pedido.FindAsync(idPedido);
            if (pedido == null) return false;

            var diferencia = Math.Abs(pedido.MontoTotal - montoVerificacion);
            return diferencia <= 0.5m; // Margen de ±S/0.5
        }

        public async Task<decimal> CalcularMontoTotal(int idPedido)
        {
            var detalles = await _bd.PedidoDetalle
                .Where(pd => pd.IdPedido == idPedido)
                .ToListAsync();

            return detalles.Sum(d => d.SubTotal ?? 0);
        }

        public async Task<bool> Guardar()
        {
            return await _bd.SaveChangesAsync() >= 0;
        }
    }
}
