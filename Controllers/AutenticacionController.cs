using ApiVentas.Modelos;
using ApiVentas.Modelos.ApiResponse;
using ApiVentas.Modelos.Dtos;
using ApiVentas.Repositorio.IRepositorio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace ApiVentas.Controllers
{
    [Route("api/v1/autenticacion")]
    [ApiController]
    public class AutenticacionController : ControllerBase
    {
        private readonly IUsuarioSistemaRepositorio _usuarioSistemaRepo;
        private readonly IConfiguration _configuration;

        public AutenticacionController(IUsuarioSistemaRepositorio usuarioSistemaRepo, IConfiguration configuration)
        {
            _usuarioSistemaRepo = usuarioSistemaRepo;
            _configuration = configuration;
        }

        /// <summary>
        /// Autenticar usuario y obtener token JWT con roles y permisos
        /// </summary>
        /// <param name="loginRequest">Credenciales de usuario</param>
        /// <returns>Token de autenticación, información del usuario, roles y permisos</returns>
        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponse<AutenticacionRespuestaDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Login([FromBody] AutenticacionLoginDto loginRequest)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    var validationResponse = ApiResponse<object>.ErrorResponse(
                        "Error de validación",
                        errors
                    );
                    validationResponse.Path = Request.Path;
                    return BadRequest(validationResponse);
                }

                // Verificar credenciales
                var usuario = await _usuarioSistemaRepo.LoginAsync(loginRequest.Correo, loginRequest.Password);
                
                if (usuario == null)
                {
                    var unauthorizedResponse = ApiResponse<object>.ErrorResponse(
                        "Credenciales inválidas",
                        "Correo o contraseña incorrectos"
                    );
                    unauthorizedResponse.Path = Request.Path;
                    return Unauthorized(unauthorizedResponse);
                }

                // Obtener roles y permisos del usuario
                var roles = await _usuarioSistemaRepo.ObtenerRolesUsuarioAsync(usuario.IdUsuario);
                var permisos = await _usuarioSistemaRepo.ObtenerPermisosUsuarioAsync(usuario.IdUsuario);

                // Generar token JWT
                var token = await GenerateJwtTokenAsync(usuario, roles, permisos);
                var expiresIn = 3600; // 1 hora en segundos
                var fechaExpiracion = DateTime.UtcNow.AddHours(1);

                var usuarioInfo = new UsuarioInfoDto
                {
                    Id = usuario.IdUsuario,
                    CodigoEquivalencia = usuario.CodigoEquivalencia,
                    NombreCompleto = usuario.NombreCompleto,
                    Correo = usuario.Correo,
                    EstadoRegistro = usuario.EstadoRegistro,
                    FechaCreacion = usuario.FechaCreacion,
                    Roles = roles,
                    Permisos = permisos
                };

                var authResponse = new AutenticacionRespuestaDto
                {
                    Token = token,
                    ExpiraEn = expiresIn,
                    FechaExpiracion = fechaExpiracion,
                    Usuario = usuarioInfo
                };

                var response = ApiResponse<AutenticacionRespuestaDto>.SuccessResponse(
                    authResponse,
                    "Autenticación exitosa"
                );
                response.Path = Request.Path;

                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.ErrorResponse(
                    "Error interno del servidor",
                    ex.Message
                );
                errorResponse.Path = Request.Path;
                return StatusCode(500, errorResponse);
            }
        }

        /// <summary>
        /// Registrar nuevo usuario con roles asignados
        /// </summary>
        /// <param name="registerRequest">Datos del nuevo usuario</param>
        /// <returns>Usuario creado y token de autenticación</returns>
        [HttpPost("registro")]
        [ProducesResponseType(typeof(ApiResponse<AutenticacionRespuestaDto>), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Registro([FromBody] AutenticacionRegistroDto registerRequest)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    var validationResponse = ApiResponse<object>.ErrorResponse(
                        "Error de validación",
                        errors
                    );
                    validationResponse.Path = Request.Path;
                    return BadRequest(validationResponse);
                }

                // Verificar si el correo ya existe
                if (await _usuarioSistemaRepo.ExisteUsuarioCorreoAsync(registerRequest.Correo))
                {
                    var conflictResponse = ApiResponse<object>.ErrorResponse(
                        "Correo ya existe",
                        $"Ya existe un usuario con el correo {registerRequest.Correo}"
                    );
                    conflictResponse.Path = Request.Path;
                    return Conflict(conflictResponse);
                }

                // Validar que los roles existan
                var todosLosRoles = await _usuarioSistemaRepo.ObtenerTodosLosRolesAsync();
                var rolesValidos = todosLosRoles.Where(r => registerRequest.RolesIds.Contains(r.IdRol)).ToList();
                
                if (rolesValidos.Count != registerRequest.RolesIds.Count)
                {
                    var badRequestResponse = ApiResponse<object>.ErrorResponse(
                        "Roles inválidos",
                        "Uno o más roles especificados no existen"
                    );
                    badRequestResponse.Path = Request.Path;
                    return BadRequest(badRequestResponse);
                }

                // Crear nuevo usuario
                var nuevoUsuario = new UsuarioSistema
                {
                    NombreCompleto = registerRequest.NombreCompleto,
                    Correo = registerRequest.Correo,
                    PasswordHash = registerRequest.Password, // Se encriptará en el repositorio
                    EstadoRegistro = true,
                    FechaCreacion = DateTime.UtcNow
                };

                var usuarioCreado = await _usuarioSistemaRepo.CrearUsuarioAsync(nuevoUsuario);

                if (usuarioCreado == null)
                {
                    var errorResponse = ApiResponse<object>.ErrorResponse(
                        "Error al crear usuario",
                        "No se pudo registrar el usuario en la base de datos"
                    );
                    errorResponse.Path = Request.Path;
                    return StatusCode(500, errorResponse);
                }

                // Asignar roles al usuario
                await _usuarioSistemaRepo.AsignarRolesUsuarioAsync(usuarioCreado.IdUsuario, registerRequest.RolesIds);

                // Obtener roles y permisos del usuario recién creado
                var roles = await _usuarioSistemaRepo.ObtenerRolesUsuarioAsync(usuarioCreado.IdUsuario);
                var permisos = await _usuarioSistemaRepo.ObtenerPermisosUsuarioAsync(usuarioCreado.IdUsuario);

                // Generar token para el nuevo usuario
                var token = await GenerateJwtTokenAsync(usuarioCreado, roles, permisos);
                var expiresIn = 3600; // 1 hora en segundos
                var fechaExpiracion = DateTime.UtcNow.AddHours(1);

                var usuarioInfo = new UsuarioInfoDto
                {
                    Id = usuarioCreado.IdUsuario,
                    CodigoEquivalencia = usuarioCreado.CodigoEquivalencia,
                    NombreCompleto = usuarioCreado.NombreCompleto,
                    Correo = usuarioCreado.Correo,
                    EstadoRegistro = usuarioCreado.EstadoRegistro,
                    FechaCreacion = usuarioCreado.FechaCreacion,
                    Roles = roles,
                    Permisos = permisos
                };

                var authResponse = new AutenticacionRespuestaDto
                {
                    Token = token,
                    ExpiraEn = expiresIn,
                    FechaExpiracion = fechaExpiracion,
                    Usuario = usuarioInfo
                };

                var response = ApiResponse<AutenticacionRespuestaDto>.SuccessResponse(
                    authResponse,
                    "Usuario registrado exitosamente"
                );
                response.Path = Request.Path;

                return CreatedAtAction(
                    nameof(ObtenerPerfil),
                    new { },
                    response
                );
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.ErrorResponse(
                    "Error interno del servidor",
                    ex.Message
                );
                errorResponse.Path = Request.Path;
                return StatusCode(500, errorResponse);
            }
        }

        /// <summary>
        /// Obtener perfil del usuario autenticado con roles y permisos
        /// </summary>
        /// <returns>Información completa del usuario</returns>
        [HttpGet("perfil")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<UsuarioInfoDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> ObtenerPerfil()
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == 0)
                {
                    var unauthorizedResponse = ApiResponse<object>.ErrorResponse(
                        "Token inválido",
                        "No se pudo obtener la información del usuario del token"
                    );
                    unauthorizedResponse.Path = Request.Path;
                    return Unauthorized(unauthorizedResponse);
                }

                var usuario = await _usuarioSistemaRepo.ObtenerUsuarioPorIdAsync(userId);
                if (usuario == null)
                {
                    var notFoundResponse = ApiResponse<object>.ErrorResponse(
                        "Usuario no encontrado",
                        $"No se encontró el usuario con ID {userId}"
                    );
                    notFoundResponse.Path = Request.Path;
                    return NotFound(notFoundResponse);
                }

                // Obtener roles y permisos actualizados
                var roles = await _usuarioSistemaRepo.ObtenerRolesUsuarioAsync(usuario.IdUsuario);
                var permisos = await _usuarioSistemaRepo.ObtenerPermisosUsuarioAsync(usuario.IdUsuario);

                var usuarioInfo = new UsuarioInfoDto
                {
                    Id = usuario.IdUsuario,
                    CodigoEquivalencia = usuario.CodigoEquivalencia,
                    NombreCompleto = usuario.NombreCompleto,
                    Correo = usuario.Correo,
                    EstadoRegistro = usuario.EstadoRegistro,
                    FechaCreacion = usuario.FechaCreacion,
                    Roles = roles,
                    Permisos = permisos
                };

                var response = ApiResponse<UsuarioInfoDto>.SuccessResponse(
                    usuarioInfo,
                    "Perfil obtenido exitosamente"
                );
                response.Path = Request.Path;

                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.ErrorResponse(
                    "Error interno del servidor",
                    ex.Message
                );
                errorResponse.Path = Request.Path;
                return StatusCode(500, errorResponse);
            }
        }

        /// <summary>
        /// Cerrar sesión (invalidar token)
        /// </summary>
        /// <returns>Confirmación de logout</returns>
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Logout()
        {
            try
            {
                // En una implementación real, aquí podrías invalidar el token
                // guardándolo en una blacklist o usando refresh tokens
                
                var response = ApiResponse<object>.SuccessResponse(
                    "Sesión cerrada exitosamente"
                );
                response.Path = Request.Path;

                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.ErrorResponse(
                    "Error interno del servidor",
                    ex.Message
                );
                errorResponse.Path = Request.Path;
                return StatusCode(500, errorResponse);
            }
        }

        /// <summary>
        /// Validar si el token actual es válido y obtener información del usuario
        /// </summary>
        /// <returns>Estado del token con roles y permisos</returns>
        [HttpGet("validar-token")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<TokenValidacionDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.Unauthorized)]
        public async Task<IActionResult> ValidarToken()
        {
            try
            {
                var userId = GetUserIdFromToken();
                var correo = GetCorreoFromToken();

                // Obtener roles y permisos del token
                var roles = GetRolesFromToken();
                var permisos = GetPermisosFromToken();

                var tokenInfo = new TokenValidacionDto
                {
                    EsValido = true,
                    UsuarioId = userId,
                    Correo = correo,
                    ExpiraEn = GetTokenExpiration(),
                    Roles = roles,
                    Permisos = permisos
                };

                var response = ApiResponse<TokenValidacionDto>.SuccessResponse(
                    tokenInfo,
                    "Token válido"
                );
                response.Path = Request.Path;

                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.ErrorResponse(
                    "Error interno del servidor",
                    ex.Message
                );
                errorResponse.Path = Request.Path;
                return StatusCode(500, errorResponse);
            }
        }

        /// <summary>
        /// Obtener todos los roles disponibles
        /// </summary>
        /// <returns>Lista de roles</returns>
        [HttpGet("roles")]
        [ProducesResponseType(typeof(ApiResponse<List<RolDto>>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> ObtenerRoles()
        {
            try
            {
                var roles = await _usuarioSistemaRepo.ObtenerTodosLosRolesAsync();
                var rolesDto = roles.Select(r => new RolDto
                {
                    IdRol = r.IdRol,
                    Nombre = r.Nombre,
                    Descripcion = r.Descripcion
                }).ToList();

                var response = ApiResponse<List<RolDto>>.SuccessResponse(
                    rolesDto,
                    "Roles obtenidos exitosamente"
                );
                response.Path = Request.Path;

                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.ErrorResponse(
                    "Error interno del servidor",
                    ex.Message
                );
                errorResponse.Path = Request.Path;
                return StatusCode(500, errorResponse);
            }
        }

        /// <summary>
        /// Endpoint de prueba para verificar conectividad
        /// </summary>
        /// <returns>Estado de la conexión</returns>
        [HttpGet("test-connection")]
        [ProducesResponseType(typeof(ApiResponse<object>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> TestConnection()
        {
            try
            {
                var roles = await _usuarioSistemaRepo.ObtenerTodosLosRolesAsync();
                
                var response = ApiResponse<object>.SuccessResponse(
                    new { 
                        message = "Conexión exitosa",
                        rolesCount = roles.Count,
                        roles = roles.Select(r => new { r.IdRol, r.Nombre }).ToList()
                    },
                    "Base de datos conectada correctamente"
                );
                response.Path = Request.Path;

                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.ErrorResponse(
                    "Error de conexión a base de datos",
                    ex.Message
                );
                errorResponse.Path = Request.Path;
                return StatusCode(500, errorResponse);
            }
        }

        #region Métodos privados

        private async Task<string> GenerateJwtTokenAsync(UsuarioSistema usuario, List<RolDto> roles, List<PermisoDto> permisos)
        {
            var key = _configuration.GetValue<string>("ApiSettings:Secreta");
            var keyBytes = Encoding.ASCII.GetBytes(key);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                new Claim(ClaimTypes.Name, usuario.NombreCompleto),
                new Claim(ClaimTypes.Email, usuario.Correo)
            };

            // Agregar roles como claims
            foreach (var rol in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, rol.Nombre));
                claims.Add(new Claim("rol_id", rol.IdRol.ToString()));
            }

            // Agregar permisos como claims
            foreach (var permiso in permisos)
            {
                claims.Add(new Claim("permiso", $"{permiso.ModuloNombre}.{permiso.Nombre}"));
                claims.Add(new Claim("permiso_id", permiso.IdPermiso.ToString()));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(keyBytes),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        private int GetUserIdFromToken()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        private string GetCorreoFromToken()
        {
            return User.FindFirst(ClaimTypes.Email)?.Value ?? "";
        }

        private List<string> GetRolesFromToken()
        {
            return User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        }

        private List<string> GetPermisosFromToken()
        {
            return User.FindAll("permiso").Select(c => c.Value).ToList();
        }

        private DateTime GetTokenExpiration()
        {
            var expClaim = User.FindFirst("exp")?.Value;
            if (long.TryParse(expClaim, out var exp))
            {
                return DateTimeOffset.FromUnixTimeSeconds(exp).DateTime;
            }
            return DateTime.UtcNow;
        }

        #endregion
    }
}