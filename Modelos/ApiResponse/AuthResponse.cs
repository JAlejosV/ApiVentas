using System.Text.Json.Serialization;

namespace ApiVentas.Modelos.ApiResponse
{
    /// <summary>
    /// Respuesta de autenticación exitosa
    /// </summary>
    public class AuthResponse
    {
        [JsonPropertyName("token")]
        public string Token { get; set; }

        [JsonPropertyName("tokenType")]
        public string TokenType { get; set; } = "Bearer";

        [JsonPropertyName("expiresIn")]
        public int ExpiresIn { get; set; } // segundos

        [JsonPropertyName("expiresAt")]
        public DateTime ExpiresAt { get; set; }

        [JsonPropertyName("user")]
        public UserInfo User { get; set; }

        public AuthResponse()
        {
        }

        public AuthResponse(string token, int expiresInSeconds, UserInfo user)
        {
            Token = token;
            ExpiresIn = expiresInSeconds;
            ExpiresAt = DateTime.UtcNow.AddSeconds(expiresInSeconds);
            User = user;
        }
    }

    /// <summary>
    /// Información del usuario autenticado
    /// </summary>
    public class UserInfo
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("roles")]
        public List<string> Roles { get; set; }

        public UserInfo()
        {
            Roles = new List<string>();
        }
    }

    /// <summary>
    /// Request para login
    /// </summary>
    public class LoginRequest
    {
        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("password")]
        public string Password { get; set; }
    }

    /// <summary>
    /// Request para registro de usuario
    /// </summary>
    public class RegisterRequest
    {
        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("password")]
        public string Password { get; set; }

        [JsonPropertyName("confirmPassword")]
        public string ConfirmPassword { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}