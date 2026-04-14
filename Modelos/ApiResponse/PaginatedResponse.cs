using System.Text.Json.Serialization;

namespace ApiVentas.Modelos.ApiResponse
{
    /// <summary>
    /// Información de paginación para listas
    /// </summary>
    public class PaginationInfo
    {
        [JsonPropertyName("currentPage")]
        public int CurrentPage { get; set; }

        [JsonPropertyName("pageSize")]
        public int PageSize { get; set; }

        [JsonPropertyName("totalPages")]
        public int TotalPages { get; set; }

        [JsonPropertyName("totalRecords")]
        public int TotalRecords { get; set; }

        [JsonPropertyName("hasNext")]
        public bool HasNext { get; set; }

        [JsonPropertyName("hasPrevious")]
        public bool HasPrevious { get; set; }

        public PaginationInfo(int currentPage, int pageSize, int totalRecords)
        {
            CurrentPage = currentPage;
            PageSize = pageSize;
            TotalRecords = totalRecords;
            TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
            HasNext = CurrentPage < TotalPages;
            HasPrevious = CurrentPage > 1;
        }
    }

    /// <summary>
    /// Respuesta paginada para listas de datos
    /// </summary>
    /// <typeparam name="T">Tipo de elementos en la lista</typeparam>
    public class PaginatedResponse<T> : ApiResponse<List<T>>
    {
        [JsonPropertyName("pagination")]
        public PaginationInfo Pagination { get; set; }

        public PaginatedResponse()
        {
            Data = new List<T>();
        }

        public PaginatedResponse(List<T> items, int currentPage, int pageSize, int totalRecords)
        {
            Data = items;
            Pagination = new PaginationInfo(currentPage, pageSize, totalRecords);
        }

        /// <summary>
        /// Crear una respuesta paginada exitosa
        /// </summary>
        /// <param name="items">Lista de elementos</param>
        /// <param name="currentPage">Página actual</param>
        /// <param name="pageSize">Tamaño de página</param>
        /// <param name="totalRecords">Total de registros</param>
        /// <param name="message">Mensaje de éxito</param>
        /// <returns>Respuesta paginada exitosa</returns>
        public static PaginatedResponse<T> CreateSuccessResponse(
            List<T> items, 
            int currentPage, 
            int pageSize, 
            int totalRecords, 
            string message = "Datos obtenidos exitosamente")
        {
            return new PaginatedResponse<T>
            {
                Success = true,
                Message = message,
                Data = items,
                Pagination = new PaginationInfo(currentPage, pageSize, totalRecords),
                Timestamp = DateTime.UtcNow,
                Errors = null
            };
        }

        /// <summary>
        /// Crear una respuesta paginada de error
        /// </summary>
        /// <param name="message">Mensaje de error</param>
        /// <param name="errors">Lista de errores</param>
        /// <returns>Respuesta paginada de error</returns>
        public static PaginatedResponse<T> CreateErrorResponse(string message, List<string> errors = null)
        {
            return new PaginatedResponse<T>
            {
                Success = false,
                Message = message,
                Data = new List<T>(),
                Pagination = new PaginationInfo(1, 10, 0),
                Timestamp = DateTime.UtcNow,
                Errors = errors ?? new List<string> { message }
            };
        }
    }
}