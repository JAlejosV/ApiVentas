using ApiVentas.Data;
using ApiVentas.Modelos;
using ApiVentas.Repositorio.IRepositorio;

namespace ApiVentas.Repositorio
{
    public class ProveedorRepositorio : IProveedorRepositorio
    {
        private readonly ApplicationDbContext _bd;

        public ProveedorRepositorio(ApplicationDbContext bd)
        {
            _bd = bd;
        }

        public bool ActualizarProveedor(Proveedor proveedor)
        {
            // Usar Find para obtener la entidad rastreada y evitar conflictos
            var proveedorExistente = _bd.Proveedor.Find(proveedor.IdProveedor);
            if (proveedorExistente == null)
                return false;

            // Actualizar campos específicos
            proveedorExistente.CodigoProveedor = proveedor.CodigoProveedor;
            proveedorExistente.NombreProveedor = proveedor.NombreProveedor.Trim();
            proveedorExistente.Telefono = proveedor.Telefono;
            proveedorExistente.Contacto = proveedor.Contacto;
            proveedorExistente.EstadoRegistro = proveedor.EstadoRegistro;
            proveedorExistente.UsuarioActualizacion = proveedor.UsuarioActualizacion;
            proveedorExistente.FechaHoraActualizacion = DateTime.UtcNow;

            return Guardar();
        }

        public bool CrearProveedor(Proveedor proveedor)
        {
            proveedor.NombreProveedor = proveedor.NombreProveedor.Trim();
            _bd.Proveedor.Add(proveedor);
            return Guardar();
        }

        public bool ExisteProveedor(string nombreProveedor)
        {
            return _bd.Proveedor.Any(p => p.NombreProveedor.ToLower().Trim() == nombreProveedor.ToLower().Trim());
        }

        public bool ExisteProveedor(int idProveedor)
        {
            return _bd.Proveedor.Any(p => p.IdProveedor == idProveedor);
        }

        public bool ExisteProveedorExcluyendoId(string nombreProveedor, int idProveedor)
        {
            return _bd.Proveedor.Any(p => p.NombreProveedor.ToLower().Trim() == nombreProveedor.ToLower().Trim() && p.IdProveedor != idProveedor);
        }

        public bool ExisteCodigoProveedor(int codigoProveedor)
        {
            return _bd.Proveedor.Any(p => p.CodigoProveedor == codigoProveedor);
        }

        public bool ExisteCodigoProveedorExcluyendoId(int codigoProveedor, int idProveedor)
        {
            return _bd.Proveedor.Any(p => p.CodigoProveedor == codigoProveedor && p.IdProveedor != idProveedor);
        }

        public Proveedor GetProveedor(int idProveedor)
        {
            return _bd.Proveedor.FirstOrDefault(p => p.IdProveedor == idProveedor);
        }

        public ICollection<Proveedor> GetProveedores()
        {
            return _bd.Proveedor.OrderBy(p => p.NombreProveedor).ToList();
        }

        public bool Guardar()
        {
            return _bd.SaveChanges() >= 0 ? true : false;
        }

        public async Task<bool> AnularProveedor(int idProveedor)
        {
            var proveedor = await _bd.Proveedor.FindAsync(idProveedor);
            if (proveedor == null)
                return false;

            proveedor.EstadoRegistro = false;
            proveedor.UsuarioActualizacion = "SYSTEM";
            proveedor.FechaHoraActualizacion = DateTime.UtcNow;
            return Guardar();
        }

        public async Task<bool> HabilitarProveedor(int idProveedor)
        {
            var proveedor = await _bd.Proveedor.FindAsync(idProveedor);
            if (proveedor == null)
                return false;

            proveedor.EstadoRegistro = true;
            proveedor.UsuarioActualizacion = "SYSTEM";
            proveedor.FechaHoraActualizacion = DateTime.UtcNow;
            return Guardar();
        }

        // Implementación de métodos async adicionales
        public async Task<bool> ExisteProveedorAsync(int idProveedor)
        {
            return await Task.FromResult(_bd.Proveedor.Any(p => p.IdProveedor == idProveedor));
        }

        public async Task<bool> ExisteProveedorAsync(string nombreProveedor)
        {
            return await Task.FromResult(_bd.Proveedor.Any(p => p.NombreProveedor.ToLower().Trim() == nombreProveedor.ToLower().Trim()));
        }
    }
}
