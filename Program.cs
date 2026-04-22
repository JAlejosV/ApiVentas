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
//Inicio Railway 
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
//Fin Railway
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
        var auth     = new Renci.SshNet.PasswordAuthenticationMethod(sshUser ?? "", sshPwd ?? "");
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
app.Run();
