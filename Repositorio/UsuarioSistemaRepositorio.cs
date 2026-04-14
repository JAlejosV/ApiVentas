using ApiVentas.Data;
using ApiVentas.Modelos;
using ApiVentas.Modelos.Dtos;
using ApiVentas.Repositorio.IRepositorio;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace ApiVentas.Repositorio
{
    public class UsuarioSistemaRepositorio : IUsuarioSistemaRepositorio
    {
        private readonly ApplicationDbContext _context;

        public UsuarioSistemaRepositorio(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UsuarioSistema> LoginAsync(string correo, string password)
        {
            var usuario = await _context.UsuarioSistema
                .FirstOrDefaultAsync(u => u.Correo.ToLower() == correo.ToLower() && u.EstadoRegistro);

            if (usuario != null && VerificarPassword(password, usuario.PasswordHash))
            {
                return usuario;
            }

            return null;
        }

        public async Task<bool> ExisteUsuarioCorreoAsync(string correo)
        {
            return await _context.UsuarioSistema
                .AnyAsync(u => u.Correo.ToLower() == correo.ToLower());
        }

        public async Task<UsuarioSistema> CrearUsuarioAsync(UsuarioSistema usuario)
        {
            usuario.PasswordHash = HashPassword(usuario.PasswordHash);
            usuario.FechaCreacion = DateTime.UtcNow;

            _context.UsuarioSistema.Add(usuario);
            await _context.SaveChangesAsync();
            return usuario;
        }

        public async Task<UsuarioSistema> ObtenerUsuarioPorIdAsync(int id)
        {
            return await _context.UsuarioSistema
                .Include(u => u.UsuarioRoles)
                .ThenInclude(ur => ur.Rol)
                .FirstOrDefaultAsync(u => u.IdUsuario == id && u.EstadoRegistro);
        }

        public async Task<UsuarioSistema> ObtenerUsuarioPorCorreoAsync(string correo)
        {
            return await _context.UsuarioSistema
                .Include(u => u.UsuarioRoles)
                .ThenInclude(ur => ur.Rol)
                .FirstOrDefaultAsync(u => u.Correo.ToLower() == correo.ToLower() && u.EstadoRegistro);
        }

        public async Task<bool> ActualizarUsuarioAsync(UsuarioSistema usuario)
        {
            _context.UsuarioSistema.Update(usuario);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> EliminarUsuarioAsync(int id)
        {
            var usuario = await _context.UsuarioSistema.FindAsync(id);
            if (usuario != null)
            {
                usuario.EstadoRegistro = false;
                return await _context.SaveChangesAsync() > 0;
            }
            return false;
        }

        public async Task<List<RolDto>> ObtenerRolesUsuarioAsync(int usuarioId)
        {
            return await _context.UsuarioRol
                .Where(ur => ur.IdUsuario == usuarioId)
                .Include(ur => ur.Rol)
                .Select(ur => new RolDto
                {
                    IdRol = ur.Rol.IdRol,
                    Nombre = ur.Rol.Nombre,
                    Descripcion = ur.Rol.Descripcion
                })
                .ToListAsync();
        }

        public async Task<List<PermisoDto>> ObtenerPermisosUsuarioAsync(int usuarioId)
        {
            return await _context.UsuarioRol
                .Where(ur => ur.IdUsuario == usuarioId)
                .Include(ur => ur.Rol)
                .ThenInclude(r => r.RolPermisos)
                .ThenInclude(rp => rp.Permiso)
                .ThenInclude(p => p.Modulo)
                .SelectMany(ur => ur.Rol.RolPermisos)
                .Select(rp => new PermisoDto
                {
                    IdPermiso = rp.Permiso.IdPermiso,
                    Nombre = rp.Permiso.Nombre,
                    Descripcion = rp.Permiso.Descripcion,
                    ModuloNombre = rp.Permiso.Modulo.Nombre,
                    IdModulo = rp.Permiso.IdModulo
                })
                .Distinct()
                .ToListAsync();
        }

        public async Task<bool> AsignarRolesUsuarioAsync(int usuarioId, List<int> rolesIds)
        {
            // Eliminar roles existentes
            var rolesExistentes = await _context.UsuarioRol
                .Where(ur => ur.IdUsuario == usuarioId)
                .ToListAsync();
            
            _context.UsuarioRol.RemoveRange(rolesExistentes);

            // Agregar nuevos roles
            foreach (var rolId in rolesIds)
            {
                _context.UsuarioRol.Add(new UsuarioRol
                {
                    IdUsuario = usuarioId,
                    IdRol = rolId,
                    FechaAsignacion = DateTime.UtcNow
                });
            }

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> RemoverRolesUsuarioAsync(int usuarioId, List<int> rolesIds)
        {
            var rolesToRemover = await _context.UsuarioRol
                .Where(ur => ur.IdUsuario == usuarioId && rolesIds.Contains(ur.IdRol))
                .ToListAsync();

            _context.UsuarioRol.RemoveRange(rolesToRemover);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<Rol>> ObtenerTodosLosRolesAsync()
        {
            return await _context.Rol.ToListAsync();
        }

        public async Task<Rol> ObtenerRolPorIdAsync(int id)
        {
            return await _context.Rol.FindAsync(id);
        }

        public string HashPassword(string password)
        {
            // Usar BCrypt es la mejor práctica, pero para simplicidad usamos SHA256 con salt
            using (var sha256 = SHA256.Create())
            {
                var salt = "ApiVentasSalt2024"; // En producción, usar un salt único por usuario
                var passwordWithSalt = password + salt;
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(passwordWithSalt));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        public bool VerificarPassword(string password, string hash)
        {
            var hashedPassword = HashPassword(password);
            return hashedPassword == hash;
        }
    }
}