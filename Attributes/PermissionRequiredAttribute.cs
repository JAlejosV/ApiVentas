using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace ApiVentas.Attributes
{
    /// <summary>
    /// Atributo de autorización personalizado que verifica permisos específicos 
    /// basados en las claims del token JWT del usuario autenticado
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class PermissionRequiredAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string _module;
        private readonly string _permission;

        /// <summary>
        /// Constructor para especificar el módulo y permiso requerido
        /// </summary>
        /// <param name="module">Nombre del módulo (ej: "Proveedores", "Productos")</param>
        /// <param name="permission">Nombre del permiso (ej: "VerListado", "Crear", "Editar")</param>
        public PermissionRequiredAttribute(string module, string permission)
        {
            _module = module ?? throw new ArgumentNullException(nameof(module));
            _permission = permission ?? throw new ArgumentNullException(nameof(permission));
        }

        /// <summary>
        /// Método de autorización que se ejecuta antes del endpoint
        /// </summary>
        /// <param name="context">Contexto de autorización</param>
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // Verificar si el usuario está autenticado
            if (!context.HttpContext.User.Identity.IsAuthenticated)
            {
                context.Result = new UnauthorizedObjectResult(new
                {
                    success = false,
                    message = "No autorizado",
                    errors = new[] { "Debe estar autenticado para acceder a este recurso" },
                    timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    path = context.HttpContext.Request.Path.ToString()
                });
                return;
            }

            // Obtener permisos del usuario desde las claims del token JWT
            var userPermissions = context.HttpContext.User
                .FindAll("permiso")
                .Select(c => c.Value)
                .ToList();

            // Construir el permiso requerido en el formato esperado: "Modulo.Permiso"
            var requiredPermission = $"{_module}.{_permission}";
            
            // Verificar si el usuario tiene el permiso requerido
            if (!userPermissions.Contains(requiredPermission, StringComparer.OrdinalIgnoreCase))
            {
                // Obtener información adicional del usuario para el log
                var userName = context.HttpContext.User.FindFirst(ClaimTypes.Name)?.Value ?? "Usuario desconocido";
                var userRoles = context.HttpContext.User
                    .FindAll(ClaimTypes.Role)
                    .Select(c => c.Value)
                    .ToList();

                context.Result = new ObjectResult(new
                {
                    success = false,
                    message = "Acceso denegado",
                    errors = new[] { 
                        $"No tiene permisos para realizar la acción '{_permission}' en el módulo '{_module}'",
                        $"Permiso requerido: {requiredPermission}",
                        $"Roles del usuario: [{string.Join(", ", userRoles)}]"
                    },
                    timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    path = context.HttpContext.Request.Path.ToString()
                })
                {
                    StatusCode = 403
                };
                return;
            }

            // Si llegamos aquí, el usuario tiene el permiso requerido
            // La ejecución continúa normalmente
        }
    }
}