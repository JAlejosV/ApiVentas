using ApiVentas.Modelos;
using ApiVentas.Modelos.Dtos;

namespace ApiVentas.Repositorio.IRepositorio
{
    public interface IUsuarioSistemaRepositorio
    {
        // Autenticación
        Task<UsuarioSistema> LoginAsync(string correo, string password);
        Task<bool> ExisteUsuarioCorreoAsync(string correo);
        
        // CRUD Usuario
        Task<UsuarioSistema> CrearUsuarioAsync(UsuarioSistema usuario);
        Task<UsuarioSistema> ObtenerUsuarioPorIdAsync(int id);
        Task<UsuarioSistema> ObtenerUsuarioPorCorreoAsync(string correo);
        Task<List<UsuarioSistema>> ObtenerUsuariosActivosAsync();
        Task<bool> ActualizarUsuarioAsync(UsuarioSistema usuario);
        Task<bool> EliminarUsuarioAsync(int id);
        
        // Roles y Permisos
        Task<List<RolDto>> ObtenerRolesUsuarioAsync(int usuarioId);
        Task<List<PermisoDto>> ObtenerPermisosUsuarioAsync(int usuarioId);
        Task<bool> AsignarRolesUsuarioAsync(int usuarioId, List<int> rolesIds);
        Task<bool> RemoverRolesUsuarioAsync(int usuarioId, List<int> rolesIds);
        
        // Roles
        Task<List<Rol>> ObtenerTodosLosRolesAsync();
        Task<Rol> ObtenerRolPorIdAsync(int id);
        
        // Utilities
        string HashPassword(string password);
        bool VerificarPassword(string password, string hash);
    }
}