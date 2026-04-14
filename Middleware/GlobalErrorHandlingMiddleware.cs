using ApiVentas.Modelos.ApiResponse;
using System.Net;
using System.Text.Json;

namespace ApiVentas.Middleware
{
    /// <summary>
    /// Middleware para manejo global de errores
    /// </summary>
    public class GlobalErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalErrorHandlingMiddleware> _logger;

        public GlobalErrorHandlingMiddleware(RequestDelegate next, ILogger<GlobalErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocurrió un error no controlado: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            
            var response = new ApiResponse<object>
            {
                Success = false,
                Message = "Error interno del servidor",
                Timestamp = DateTime.UtcNow,
                Path = context.Request.Path
            };

            switch (exception)
            {
                case ArgumentException argEx:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = "Parámetros inválidos";
                    response.Errors.Add(argEx.Message);
                    break;

                case UnauthorizedAccessException:
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    response.Message = "Acceso no autorizado";
                    response.Errors.Add("Token inválido o expirado");
                    break;

                case KeyNotFoundException:
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    response.Message = "Recurso no encontrado";
                    response.Errors.Add(exception.Message);
                    break;

                case InvalidOperationException invOpEx:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = "Operación inválida";
                    response.Errors.Add(invOpEx.Message);
                    break;

                case TimeoutException:
                    context.Response.StatusCode = (int)HttpStatusCode.RequestTimeout;
                    response.Message = "Tiempo de espera agotado";
                    response.Errors.Add("La operación tardó demasiado tiempo en completarse");
                    break;

                default:
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    response.Message = "Error interno del servidor";
                    #if DEBUG
                    response.Errors.Add($"Detalle del error: {exception.Message}");
                    response.Errors.Add($"Stack trace: {exception.StackTrace}");
                    #else
                    response.Errors.Add("Ha ocurrido un error inesperado. Por favor, contacte al administrador.");
                    #endif
                    break;
            }

            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });

            await context.Response.WriteAsync(jsonResponse);
        }
    }

    /// <summary>
    /// Extensión para registrar el middleware
    /// </summary>
    public static class GlobalErrorHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalErrorHandling(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<GlobalErrorHandlingMiddleware>();
        }
    }
}