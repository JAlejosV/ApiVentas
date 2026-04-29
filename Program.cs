using ApiVentas.Data;
using ApiVentas.Filters;
using ApiVentas.Mappers;
using ApiVentas.Repositorio;
using ApiVentas.Repositorio.IRepositorio;
using ApiVentas.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
//Set Port: appsettings.json > variable de entorno PORT > 8080
var port = builder.Configuration["ApiSettings:Port"]
          ?? Environment.GetEnvironmentVariable("PORT")
          ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

//Configuramos la conexion a PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(opciones =>
{
    opciones.UseNpgsql(builder.Configuration.GetConnectionString("ConexionSql"),
        options => options.EnableRetryOnFailure())
        .EnableSensitiveDataLogging(); // Solo para desarrollo
});

// Configurar AppContext para usar UTC por defecto
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// Add services to the container.
builder.Services.AddScoped<IPostRepositorio, PostRepositorio>();
builder.Services.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();
builder.Services.AddScoped<IUsuarioSistemaRepositorio, UsuarioSistemaRepositorio>();
builder.Services.AddScoped<IProveedorRepositorio, ProveedorRepositorio>();
builder.Services.AddScoped<IProveedorArchivoRepositorio, ProveedorArchivoRepositorio>();
builder.Services.AddScoped<IProductoProveedorRepositorio, ProductoProveedorRepositorio>();
builder.Services.AddScoped<IProductoRepositorio, ProductoRepositorio>();
builder.Services.AddScoped<IEstadoRepositorio, EstadoRepositorio>();
builder.Services.AddScoped<IUnidadMedidaRepositorio, UnidadMedidaRepositorio>();
builder.Services.Configure<MariaDbSettings>(builder.Configuration.GetSection("MariaDb"));
builder.Services.AddScoped<IReporteRepositorio, ReporteRepositorio>();
builder.Services.AddScoped<IAlmacenRepositorio, AlmacenRepositorio>();
builder.Services.AddScoped<IPedidoRepositorio, PedidoRepositorio>();
builder.Services.AddScoped<IPedidoDetalleAlmacenRepositorio, PedidoDetalleAlmacenRepositorio>();

// Servicios
builder.Services.Configure<FileStorageSettings>(builder.Configuration.GetSection("FileStorage"));
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();

var key = builder.Configuration.GetValue<string>("ApiSettings:Secreta");

//Agregar Automapper
builder.Services.AddAutoMapper(typeof(BlogMapper));

//Aqu� se configura la Autenticaci�n - Primera parte
builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

// Configurar límites para formularios multipart
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 20 * 1024 * 1024; // 20 MB
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartHeadersLengthLimit = int.MaxValue;
});

builder.Services.AddControllers().AddNewtonsoftJson();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();


//Aqu� se configura la autenticaci�n y autorizaci�n segunda parte
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Ingresa tu token JWT. No incluyas el prefijo 'Bearer', Swagger lo agrega automáticamente.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Scheme = "Bearer",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT"
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
    options.OperationFilter<MultiFileUploadOperationFilter>();
});

//Soporte para CORS
//Se pueden habilitar: 1-Un dominio, 2-multiples dominios,
//3-cualquier dominio (Tener en cuenta seguridad)
//Usamos de ejemplo el dominio: http://localhost:3223, se debe cambiar por el correcto
//Se usa (*) para todos los dominios
builder.Services.AddCors(p => p.AddPolicy("PolicyCors", build =>
{
    build.WithOrigins("*").AllowAnyMethod().AllowAnyHeader();
}));

var app = builder.Build();

// ── Endpoints de diagnóstico (sin autenticación) ─────────────────────────────
app.MapGet("/debug-config", (IConfiguration config) =>
    config["MariaDb:ConnectionString"] ?? "NOT SET"
).ExcludeFromDescription();

app.MapGet("/debug-ssh", async (IConfiguration config) =>
{
    var cs = config["MariaDb:ConnectionString"] ?? "";
    if (string.IsNullOrWhiteSpace(cs))
        return Results.Ok(new { ok = false, step = "config", error = "MariaDb:ConnectionString no definida" });

    var parts = cs.Split(';', StringSplitOptions.RemoveEmptyEntries)
                  .Select(p => p.Split('=', 2)).Where(p => p.Length == 2)
                  .ToDictionary(p => p[0].Trim(), p => p[1].Trim(), StringComparer.OrdinalIgnoreCase);

    if (!parts.TryGetValue("SshHostName", out var sshHost) || string.IsNullOrWhiteSpace(sshHost))
        return Results.Ok(new { ok = false, step = "config", error = "SshHostName no encontrado en la cadena" });

    parts.TryGetValue("SshUserName", out var sshUser);
    parts.TryGetValue("SshPassword", out var sshPwd);
    int sshPort = parts.TryGetValue("SshPort", out var sshPortStr) && int.TryParse(sshPortStr, out var sp) ? sp : 22;

    try
    {
        var auth = new Renci.SshNet.PasswordAuthenticationMethod(sshUser ?? "", sshPwd ?? "");
        var connInfo = new Renci.SshNet.ConnectionInfo(sshHost, sshPort, sshUser ?? "", auth);
        using var ssh = new Renci.SshNet.SshClient(connInfo);
        ssh.HostKeyReceived += (_, e) => { e.CanTrust = true; };
        ssh.Connect();
        var connected = ssh.IsConnected;
        ssh.Disconnect();
        return Results.Ok(new { ok = connected, step = "ssh", host = sshHost, port = sshPort, user = sshUser });
    }
    catch (Exception ex)
    {
        return Results.Ok(new { ok = false, step = "ssh", host = sshHost, port = sshPort, error = ex.Message });
    }
}).ExcludeFromDescription();
// ─────────────────────────────────────────────────────────────────────────────


//Imagenes Locales
////Importante para habilitar que se  exponga el directorio de imagenes
////Sin esto no se puede acceder
//app.UseStaticFiles(new StaticFileOptions()
//{
//    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), @"ImagenesPosts")),
//    RequestPath = new PathString("/ImagenesPosts")
//});

////Directorio para archivos de proveedores
//app.UseStaticFiles(new StaticFileOptions()
//{
//    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\archivos")),
//    RequestPath = new PathString("/archivos")
//});
// ✅ AGREGAR ESTO: Crear directorios si no existen (necesario en Railway)
var imagenesPath = Path.Combine(Directory.GetCurrentDirectory(), "ImagenesPosts");
var archivosPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "archivos");

//Inicio Raily
if (!Directory.Exists(imagenesPath))
    Directory.CreateDirectory(imagenesPath);

if (!Directory.Exists(archivosPath))
    Directory.CreateDirectory(archivosPath);

// Configure the HTTP request pipeline.
//Inicio Railway - Swagger siempre activo
app.UseSwagger();
app.UseSwaggerUI();
//Fin Railway

// ✅ CAMBIO: Usar las variables en lugar de rutas hardcodeadas con @"..."
app.UseStaticFiles(new StaticFileOptions()
{
    FileProvider = new PhysicalFileProvider(imagenesPath),
    RequestPath = new PathString("/ImagenesPosts")
});

// ✅ CAMBIO: Usar Path.Combine en lugar de backslash \ (falla en Linux)
app.UseStaticFiles(new StaticFileOptions()
{
    FileProvider = new PhysicalFileProvider(archivosPath),
    RequestPath = new PathString("/archivos")
});
//Fin Raily

//Soporte para CORS
app.UseCors("PolicyCors");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ── Página de inicio ──────────────────────────────────────────────────────────
app.MapGet("/", (IConfiguration config) =>
{
    var port    = config["ApiSettings:Port"] ?? "8080";
    var version = "1.0";
    var now     = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
    var html    = $$"""
        <!DOCTYPE html>
        <html lang="es">
        <head>
            <meta charset="UTF-8"/>
            <meta name="viewport" content="width=device-width, initial-scale=1.0"/>
            <title>ApiVentas</title>
            <style>
                * { box-sizing: border-box; margin: 0; padding: 0; }
                body { font-family: 'Segoe UI', sans-serif; background: #0f172a; color: #e2e8f0; display: flex; align-items: center; justify-content: center; min-height: 100vh; }
                .card { background: #1e293b; border-radius: 16px; padding: 2.5rem 3rem; max-width: 480px; width: 100%; box-shadow: 0 25px 50px rgba(0,0,0,0.5); border: 1px solid #334155; }
                h1 { font-size: 1.8rem; color: #38bdf8; margin-bottom: .25rem; }
                .sub { color: #94a3b8; font-size: .9rem; margin-bottom: 2rem; }
                .badge { display: inline-block; background: #22c55e22; color: #22c55e; border: 1px solid #22c55e55; border-radius: 999px; padding: .2rem .75rem; font-size: .8rem; font-weight: 600; margin-bottom: 2rem; }
                .info { background: #0f172a; border-radius: 10px; padding: 1rem 1.25rem; margin-bottom: 1.5rem; display: flex; flex-direction: column; gap: .5rem; }
                .row { display: flex; justify-content: space-between; font-size: .875rem; }
                .label { color: #94a3b8; }
                .value { color: #e2e8f0; font-weight: 500; }
                .links { display: flex; gap: .75rem; }
                a.btn { flex: 1; text-align: center; padding: .65rem; border-radius: 8px; text-decoration: none; font-size: .875rem; font-weight: 600; transition: opacity .2s; }
                a.btn:hover { opacity: .85; }
                .btn-swagger { background: #38bdf8; color: #0f172a; }
                .btn-health  { background: #334155; color: #e2e8f0; border: 1px solid #475569; }
            </style>
        </head>
        <body>
            <div class="card">
                <h1>ApiVentas</h1>
                <p class="sub">Sistema de Ventas — API REST</p>
                <span class="badge">&#x25CF; En línea</span>
                <div class="info">
                    <div class="row"><span class="label">Versión</span>   <span class="value">{{version}}</span></div>
                    <div class="row"><span class="label">Puerto</span>    <span class="value">{{port}}</span></div>
                    <div class="row"><span class="label">Servidor</span>  <span class="value">{{Environment.MachineName}}</span></div>
                    <div class="row"><span class="label">Inicio</span>    <span class="value">{{now}}</span></div>
                </div>
                <div class="links">
                    <a class="btn btn-swagger" href="/swagger" target="_blank">Swagger UI</a>
                    <a class="btn btn-health"  href="/health"  target="_blank">Health Check</a>
                </div>
            </div>
        </body>
        </html>
        """;
    return Results.Content(html, "text/html");
}).ExcludeFromDescription();
// ─────────────────────────────────────────────────────────────────────────────


app.MapGet("/health", async (IConfiguration config) =>
{
    var resultado = new
    {
        api        = "ok",
        version    = "1.0",
        timestamp  = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " UTC",
        port       = config["ApiSettings:Port"] ?? "8080",
        mariadb    = (object)null,
        postgresql = (object)null
    };

    // ── Verificar MariaDB ─────────────────────────────────────────────────
    string mariadbStatus;
    string mariadbError = null;
    try
    {
        var cs = config["MariaDb:ConnectionString"] ?? "";
        var parts = cs.Split(';', StringSplitOptions.RemoveEmptyEntries)
                      .Select(p => p.Split('=', 2)).Where(p => p.Length == 2)
                      .ToDictionary(p => p[0].Trim(), p => p[1].Trim(), StringComparer.OrdinalIgnoreCase);

        bool useSsh = parts.TryGetValue("SshHostName", out var sshHost) && !string.IsNullOrWhiteSpace(sshHost);
        Renci.SshNet.SshClient ssh = null;
        Renci.SshNet.ForwardedPortLocal portFwd = null;
        MySqlConnector.MySqlConnection conn = null;
        try
        {
            string connStr;
            if (useSsh)
            {
                parts.TryGetValue("SshUserName", out var sshUser);
                parts.TryGetValue("SshPassword", out var sshPwd);
                int sshPort = parts.TryGetValue("SshPort", out var sshPortStr) && int.TryParse(sshPortStr, out var sp) ? sp : 22;
                var auth     = new Renci.SshNet.PasswordAuthenticationMethod(sshUser ?? "", sshPwd ?? "");
                var ci       = new Renci.SshNet.ConnectionInfo(sshHost, sshPort, sshUser ?? "", auth);
                ssh = new Renci.SshNet.SshClient(ci);
                ssh.HostKeyReceived += (_, e) => { e.CanTrust = true; };
                ssh.Connect();
                parts.TryGetValue("Server", out var dbHost);
                uint dbPort = parts.TryGetValue("Port", out var portStr) && uint.TryParse(portStr, out var dp) ? dp : 3306;
                portFwd = new Renci.SshNet.ForwardedPortLocal("127.0.0.1", 0, dbHost ?? "127.0.0.1", dbPort);
                ssh.AddForwardedPort(portFwd);
                portFwd.Start();
                var b = new MySqlConnector.MySqlConnectionStringBuilder();
                b.Server = "127.0.0.1"; b.Port = portFwd.BoundPort;
                if (parts.TryGetValue("Database", out var db2)) b.Database = db2;
                if (parts.TryGetValue("Uid", out var uid2))     b.UserID   = uid2;
                if (parts.TryGetValue("Pwd", out var pwd2))     b.Password = pwd2;
                b.ConnectionTimeout = 10;
                connStr = b.ToString();
            }
            else
            {
                var b = new MySqlConnector.MySqlConnectionStringBuilder();
                if (parts.TryGetValue("Server",   out var s3))  b.Server   = s3;
                if (parts.TryGetValue("Port",     out var p3) && uint.TryParse(p3, out var pn)) b.Port = pn;
                if (parts.TryGetValue("Database", out var d3))  b.Database = d3;
                if (parts.TryGetValue("Uid",      out var u3))  b.UserID   = u3;
                if (parts.TryGetValue("Pwd",      out var pw3)) b.Password = pw3;
                b.ConnectionTimeout = 10;
                connStr = b.ToString();
            }
            conn = new MySqlConnector.MySqlConnection(connStr);
            await conn.OpenAsync();
            mariadbStatus = "ok";
        }
        finally
        {
            if (conn != null) await conn.DisposeAsync();
            portFwd?.Stop();
            ssh?.Disconnect(); ssh?.Dispose();
        }
    }
    catch (Exception ex) { mariadbStatus = "error"; mariadbError = ex.Message; }

    // ── Verificar PostgreSQL ──────────────────────────────────────────────
    string pgStatus;
    string pgError = null;
    try
    {
        var pgCs = config.GetConnectionString("ConexionSql") ?? "";
        await using var pgConn = new Npgsql.NpgsqlConnection(pgCs);
        await pgConn.OpenAsync();
        pgStatus = "ok";
    }
    catch (Exception ex) { pgStatus = "error"; pgError = ex.Message; }

    var allOk = mariadbStatus == "ok" && pgStatus == "ok";

    return Results.Json(new
    {
        api        = "ok",
        version    = "1.0",
        timestamp  = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " UTC",
        port       = config["ApiSettings:Port"] ?? "8080",
        mariadb    = new { status = mariadbStatus, error = mariadbError },
        postgresql = new { status = pgStatus,      error = pgError      }
    }, statusCode: allOk ? 200 : 503);
});
// ─────────────────────────────────────────────────────────────────────────────

app.Run();
