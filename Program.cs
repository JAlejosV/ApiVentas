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
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

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
    x.DefaultAuthenticateScheme= JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme= JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x =>
{
    x.RequireHttpsMetadata= false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey= true,
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
builder.Services.AddSwaggerGen();

//Aqu� se configura la autenticaci�n y autorizaci�n segunda parte
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description =
        "Autenticaci�n JWT usando el esquema Bearer. \r\n\r\n " +
        "Ingresa la palabra 'Bearer' seguida de un [espacio] y despues su token en el campo de abajo \r\n\r\n" +
        "Ejemplo: \"Bearer tkdknkdllskd\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//Importante para habilitar que se  exponga el directorio de imagenes
//Sin esto no se puede acceder
app.UseStaticFiles(new StaticFileOptions()
{
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), @"ImagenesPosts")),
    RequestPath = new PathString("/ImagenesPosts")
});

//Directorio para archivos de proveedores
app.UseStaticFiles(new StaticFileOptions()
{
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\archivos")),
    RequestPath = new PathString("/archivos")
});


//Soporte para CORS
app.UseCors("PolicyCors");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
