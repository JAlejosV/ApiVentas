using ApiVentas.Modelos;
using Microsoft.EntityFrameworkCore;

namespace ApiVentas.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {            
        }

        //Agregar los modelos
        public DbSet<Post> Post { get; set; }
        public DbSet<Usuario> Usuario { get; set; }
        
        // Sistema de autenticación con roles
        public DbSet<UsuarioSistema> UsuarioSistema { get; set; }
        public DbSet<Rol> Rol { get; set; }
        public DbSet<UsuarioRol> UsuarioRol { get; set; }
        public DbSet<Modulo> Modulo { get; set; }
        public DbSet<Permiso> Permiso { get; set; }
        public DbSet<RolPermiso> RolPermiso { get; set; }
        
        public DbSet<Proveedor> Proveedor { get; set; }
        public DbSet<ProveedorArchivo> ProveedorArchivo { get; set; }
        public DbSet<ProductoProveedor> ProductoProveedor { get; set; }
        public DbSet<Producto> Producto { get; set; }
        public DbSet<Estado> Estado { get; set; }
        public DbSet<UnidadMedida> UnidadMedida { get; set; }
        public DbSet<Almacen> Almacen { get; set; }
        public DbSet<Pedido> Pedido { get; set; }
        public DbSet<PedidoArchivo> PedidoArchivo { get; set; }
        public DbSet<PedidoDetalle> PedidoDetalle { get; set; }
        public DbSet<PedidoDetalleAlmacen> PedidoDetalleAlmacen { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurar nombres de tablas y columnas en minúsculas para PostgreSQL
            modelBuilder.Entity<Post>(entity =>
            {
                entity.ToTable("post");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Titulo).HasColumnName("titulo");
                entity.Property(e => e.Descripcion).HasColumnName("descripcion");
                entity.Property(e => e.RutaImagen).HasColumnName("rutaimagen");
                entity.Property(e => e.Etiquetas).HasColumnName("etiquetas");
                entity.Property(e => e.FechaCreacion).HasColumnName("fechacreacion");
                entity.Property(e => e.FechaActualizacion).HasColumnName("fechaactualizacion");
            });

            // SISTEMA ANTERIOR - Comentado temporalmente para evitar conflictos de tabla
            /*
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.ToTable("usuario_legacy");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.NombreUsuario).HasColumnName("nombreusuario");
                entity.Property(e => e.Nombre).HasColumnName("nombre");
                entity.Property(e => e.Email).HasColumnName("email");
                entity.Property(e => e.Password).HasColumnName("password");
            });
            */

            // Configuración del sistema de autenticación con roles
            modelBuilder.Entity<UsuarioSistema>(entity =>
            {
                entity.ToTable("usuario");
                entity.HasKey(e => e.IdUsuario);
                entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
                entity.Property(e => e.CodigoEquivalencia).HasColumnName("codigo_equivalencia");
                entity.Property(e => e.NombreCompleto).HasColumnName("nombre_completo").HasMaxLength(200);
                entity.Property(e => e.Correo).HasColumnName("correo").HasMaxLength(200);
                entity.Property(e => e.PasswordHash).HasColumnName("password_hash");
                entity.Property(e => e.EstadoRegistro).HasColumnName("estadoregistro").HasDefaultValue(true);
                entity.Property(e => e.FechaCreacion).HasColumnName("fecha_creacion").HasDefaultValueSql("NOW()");
                
                entity.HasIndex(e => e.Correo).IsUnique();
            });

            modelBuilder.Entity<Rol>(entity =>
            {
                entity.ToTable("rol");
                entity.HasKey(e => e.IdRol);
                entity.Property(e => e.IdRol).HasColumnName("id_rol");
                entity.Property(e => e.Nombre).HasColumnName("nombre").HasMaxLength(50);
                entity.Property(e => e.Descripcion).HasColumnName("descripcion");
                
                entity.HasIndex(e => e.Nombre).IsUnique();
            });

            modelBuilder.Entity<UsuarioRol>(entity =>
            {
                entity.ToTable("usuario_rol");
                entity.HasKey(e => new { e.IdUsuario, e.IdRol });
                entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
                entity.Property(e => e.IdRol).HasColumnName("id_rol");
                entity.Property(e => e.FechaAsignacion).HasColumnName("fecha_asignacion").HasDefaultValueSql("NOW()");
                
                entity.HasOne(e => e.Usuario)
                    .WithMany(u => u.UsuarioRoles)
                    .HasForeignKey(e => e.IdUsuario)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasOne(e => e.Rol)
                    .WithMany(r => r.UsuarioRoles)
                    .HasForeignKey(e => e.IdRol)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Modulo>(entity =>
            {
                entity.ToTable("modulo");
                entity.HasKey(e => e.IdModulo);
                entity.Property(e => e.IdModulo).HasColumnName("id_modulo");
                entity.Property(e => e.Nombre).HasColumnName("nombre").HasMaxLength(100);
                entity.Property(e => e.Descripcion).HasColumnName("descripcion");
                
                entity.HasIndex(e => e.Nombre).IsUnique();
            });

            modelBuilder.Entity<Permiso>(entity =>
            {
                entity.ToTable("permiso");
                entity.HasKey(e => e.IdPermiso);
                entity.Property(e => e.IdPermiso).HasColumnName("id_permiso");
                entity.Property(e => e.IdModulo).HasColumnName("id_modulo");
                entity.Property(e => e.Nombre).HasColumnName("nombre").HasMaxLength(50);
                entity.Property(e => e.Descripcion).HasColumnName("descripcion");
                
                entity.HasOne(e => e.Modulo)
                    .WithMany(m => m.Permisos)
                    .HasForeignKey(e => e.IdModulo)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasIndex(e => new { e.IdModulo, e.Nombre })
                    .IsUnique()
                    .HasDatabaseName("uq_modulo_accion");
            });

            modelBuilder.Entity<RolPermiso>(entity =>
            {
                entity.ToTable("rol_permiso");
                entity.HasKey(e => new { e.IdRol, e.IdPermiso });
                entity.Property(e => e.IdRol).HasColumnName("id_rol");
                entity.Property(e => e.IdPermiso).HasColumnName("id_permiso");
                
                entity.HasOne(e => e.Rol)
                    .WithMany(r => r.RolPermisos)
                    .HasForeignKey(e => e.IdRol)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasOne(e => e.Permiso)
                    .WithMany(p => p.RolPermisos)
                    .HasForeignKey(e => e.IdPermiso)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Almacen>(entity =>
            {
                entity.ToTable("almacen");
                entity.Property(e => e.IdAlmacen).HasColumnName("idalmacen");
                entity.Property(e => e.CodigoAlmacen).HasColumnName("codigoalmacen");
                entity.Property(e => e.NombreAlmacen).HasColumnName("nombrealmacen");
                entity.Property(e => e.EstadoRegistro).HasColumnName("estadoregistro");
            });

            modelBuilder.Entity<Proveedor>(entity =>
            {
                entity.ToTable("proveedor");
                entity.Property(e => e.IdProveedor).HasColumnName("idproveedor");
                entity.Property(e => e.CodigoProveedor).HasColumnName("codigoproveedor");
                entity.Property(e => e.NombreProveedor).HasColumnName("nombreproveedor");
                entity.Property(e => e.Telefono).HasColumnName("telefono");
                entity.Property(e => e.Contacto).HasColumnName("contacto");
                entity.Property(e => e.EstadoRegistro).HasColumnName("estadoregistro");
                entity.Property(e => e.UsuarioCreacion).HasColumnName("usuariocreacion");
                entity.Property(e => e.FechaHoraCreacion).HasColumnName("fechahoracreacion");
                entity.Property(e => e.UsuarioActualizacion).HasColumnName("usuarioactualizacion");
                entity.Property(e => e.FechaHoraActualizacion).HasColumnName("fechahoraactualizacion");
            });

            modelBuilder.Entity<Producto>(entity =>
            {
                entity.ToTable("producto");
                entity.Property(e => e.IdProducto).HasColumnName("idproducto");
                entity.Property(e => e.CodigoProducto).HasColumnName("codigoproducto");
                entity.Property(e => e.NombreProducto).HasColumnName("nombreproducto");
                entity.Property(e => e.StockReal).HasColumnName("stockreal");
                entity.Property(e => e.StockMinimo).HasColumnName("stockminimo");
                entity.Property(e => e.EstadoRegistro).HasColumnName("estadoregistro");
                entity.Property(e => e.UsuarioCreacion).HasColumnName("usuariocreacion");
                entity.Property(e => e.FechaHoraCreacion).HasColumnName("fechahoracreacion");
                entity.Property(e => e.UsuarioActualizacion).HasColumnName("usuarioactualizacion");
                entity.Property(e => e.FechaHoraActualizacion).HasColumnName("fechahoraactualizacion");
            });

            modelBuilder.Entity<Estado>(entity =>
            {
                entity.ToTable("estado");
                entity.Property(e => e.IdEstado).HasColumnName("idestado");
                entity.Property(e => e.CodigoEstado).HasColumnName("codigoestado");
                entity.Property(e => e.NombreEstado).HasColumnName("nombreestado");
                entity.Property(e => e.DescripcionEstado).HasColumnName("descripcionestado");
                entity.Property(e => e.EstadoRegistro).HasColumnName("estadoregistro");
            });

            modelBuilder.Entity<UnidadMedida>(entity =>
            {
                entity.ToTable("unidadmedida");
                entity.HasKey(e => e.IdUnidadMedida);
                entity.Property(e => e.IdUnidadMedida).HasColumnName("idunidadmedida");
                entity.Property(e => e.CodigoUnidadMedida).HasColumnName("codigounidadmedida").HasMaxLength(10);
                entity.Property(e => e.NombreUnidadMedida).HasColumnName("nombreunidadmedida").HasMaxLength(100);
                entity.Property(e => e.EstadoRegistro).HasColumnName("estadoregistro").HasDefaultValue(true);
            });

            modelBuilder.Entity<ProveedorArchivo>(entity =>
            {
                entity.ToTable("proveedorarchivo");
                entity.Property(e => e.IdProveedorArchivo).HasColumnName("idproveedorarchivo");
                entity.Property(e => e.IdProveedor).HasColumnName("idproveedor");
                entity.Property(e => e.NombreArchivo).HasColumnName("nombrearchivo");
                entity.Property(e => e.RutaArchivo).HasColumnName("rutaarchivo");
            });

            modelBuilder.Entity<ProductoProveedor>(entity =>
            {
                entity.ToTable("productoproveedor");
                entity.Property(e => e.IdProductoProveedor).HasColumnName("idproductoproveedor");
                entity.Property(e => e.IdProveedor).HasColumnName("idproveedor");
                entity.Property(e => e.IdProducto).HasColumnName("idproducto");
                entity.Property(e => e.CantidadPorPaquete).HasColumnName("cantidadporpaquete");
                entity.Property(e => e.PrecioPorPaquete).HasColumnName("precioporpaquete");
                entity.Property(e => e.PrecioUnitario).HasColumnName("preciounitario");
                entity.Property(e => e.EstadoRegistro).HasColumnName("estadoregistro");
                entity.Property(e => e.UsuarioCreacion).HasColumnName("usuariocreacion");
                entity.Property(e => e.FechaHoraCreacion).HasColumnName("fechahoracreacion");
                entity.Property(e => e.UsuarioActualizacion).HasColumnName("usuarioactualizacion");
                entity.Property(e => e.FechaHoraActualizacion).HasColumnName("fechahoraactualizacion");
            });

            modelBuilder.Entity<Pedido>(entity =>
            {
                entity.ToTable("pedido");
                entity.Property(e => e.IdPedido).HasColumnName("idpedido");
                entity.Property(e => e.IdProveedor).HasColumnName("idproveedor");
                entity.Property(e => e.IdEstado).HasColumnName("idestado");
                entity.Property(e => e.MontoTotal).HasColumnName("montototal");
                entity.Property(e => e.UsuarioCreacion).HasColumnName("usuariocreacion");
                entity.Property(e => e.FechaHoraCreacion).HasColumnName("fechahoracreacion");
                entity.Property(e => e.UsuarioActualizacion).HasColumnName("usuarioactualizacion");
                entity.Property(e => e.FechaHoraActualizacion).HasColumnName("fechahoraactualizacion");
            });

            modelBuilder.Entity<PedidoArchivo>(entity =>
            {
                entity.ToTable("pedidoarchivo");
                entity.Property(e => e.IdPedidoArchivo).HasColumnName("idpedidoarchivo");
                entity.Property(e => e.IdPedido).HasColumnName("idpedido");
                entity.Property(e => e.NombreArchivo).HasColumnName("nombrearchivo");
                entity.Property(e => e.RutaArchivo).HasColumnName("rutaarchivo");
                entity.Property(e => e.UsuarioCreacion).HasColumnName("usuariocreacion");
                entity.Property(e => e.FechaHoraCreacion).HasColumnName("fechahoracreacion");
                entity.Property(e => e.UsuarioActualizacion).HasColumnName("usuarioactualizacion");
                entity.Property(e => e.FechaHoraActualizacion).HasColumnName("fechahoraactualizacion");
            });

            modelBuilder.Entity<PedidoDetalle>(entity =>
            {
                entity.ToTable("pedidodetalle");
                entity.Property(e => e.IdPedidoDetalle).HasColumnName("idpedidodetalle");
                entity.Property(e => e.IdPedido).HasColumnName("idpedido");
                entity.Property(e => e.IdProveedor).HasColumnName("idproveedor");
                entity.Property(e => e.IdProductoProveedor).HasColumnName("idproductoproveedor");
                entity.Property(e => e.CantidadPorPaquete).HasColumnName("cantidadporpaquete");
                entity.Property(e => e.PrecioPorPaquete).HasColumnName("precioporpaquete");
                entity.Property(e => e.PedidoPorPaquete).HasColumnName("pedidoporpaquete");
                entity.Property(e => e.BonoPorPaquete).HasColumnName("bonoporpaquete");
                entity.Property(e => e.BonoPorUnidad).HasColumnName("bonoporunidad");
                entity.Property(e => e.TotalUnidades).HasColumnName("totalunidades");
                entity.Property(e => e.PrecioUnitario).HasColumnName("preciounitario");
                entity.Property(e => e.SubTotal).HasColumnName("subtotal");
                entity.Property(e => e.UsuarioCreacion).HasColumnName("usuariocreacion");
                entity.Property(e => e.FechaHoraCreacion).HasColumnName("fechahoracreacion");
                entity.Property(e => e.UsuarioActualizacion).HasColumnName("usuarioactualizacion");
                entity.Property(e => e.FechaHoraActualizacion).HasColumnName("fechahoraactualizacion");
            });

            modelBuilder.Entity<PedidoDetalleAlmacen>(entity =>
            {
                entity.ToTable("pedidodetallealmacen");
                entity.Property(e => e.IdPedidoDetalleAlmacen).HasColumnName("idpedidodetallealmacen");
                entity.Property(e => e.IdPedidoDetalle).HasColumnName("idpedidodetalle");
                entity.Property(e => e.IdAlmacen).HasColumnName("idalmacen");
                entity.Property(e => e.Cantidad).HasColumnName("cantidad");
                entity.Property(e => e.UsuarioCreacion).HasColumnName("usuariocreacion");
                entity.Property(e => e.FechaHoraCreacion).HasColumnName("fechahoracreacion");
                entity.Property(e => e.UsuarioActualizacion).HasColumnName("usuarioactualizacion");
                entity.Property(e => e.FechaHoraActualizacion).HasColumnName("fechahoraactualizacion");
            });
        }
    }
}
