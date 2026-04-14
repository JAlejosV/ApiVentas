using System.Text.Json.Serialization;

namespace ApiVentas.Modelos.ApiResponse
{
    /// <summary>
    /// Respuesta estándar para todas las APIs
    /// </summary>
    /// <typeparam name="T">Tipo de datos que contiene la respuesta</typeparam>
    public class ApiResponse<T>
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("data")]
        public T Data { get; set; }

        [JsonPropertyName("errors")]
        public List<string> Errors { get; set; }

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonPropertyName("path")]
        public string Path { get; set; }

        public ApiResponse()
        {
            Timestamp = DateTime.UtcNow;
            Errors = new List<string>();
        }

        /// <summary>
        /// Respuesta exitosa con datos
        /// </summary>
        public static ApiResponse<T> SuccessResponse(T data, string message = "Operación exitosa")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        /// <summary>
        /// Respuesta exitosa sin datos
        /// </summary>
        public static ApiResponse<T> SuccessResponse(string message = "Operación exitosa")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message
            };
        }

        /// <summary>
        /// Respuesta de error
        /// </summary>
        public static ApiResponse<T> ErrorResponse(string message, List<string> errors = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Errors = errors ?? new List<string>()
            };
        }

        /// <summary>
        /// Respuesta de error con un solo error
        /// </summary>
        public static ApiResponse<T> ErrorResponse(string message, string error)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Errors = new List<string> { error }
            };
        }
    }
}