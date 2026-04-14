using ApiVentas.Modelos;
using ApiVentas.Modelos.Dtos;
using AutoMapper;

namespace ApiVentas.Mappers
{
    public class BlogMapper : Profile
    {
        public BlogMapper()
        {
            CreateMap<Post, PostDto>().ReverseMap();
            CreateMap<Post, PostCrearDto>().ReverseMap();
            CreateMap<Post, PostActualizarDto>().ReverseMap();

            CreateMap<Proveedor, ProveedorDto>().ReverseMap();
            CreateMap<Proveedor, ProveedorCrearDto>().ReverseMap();
            CreateMap<Proveedor, ProveedorActualizarDto>().ReverseMap();

            CreateMap<ProveedorArchivo, ProveedorArchivoDto>().ReverseMap();
            CreateMap<ProveedorArchivo, ProveedorArchivoDetalleDto>().ReverseMap();

            // Mappers para ProductoProveedor
            CreateMap<ProductoProveedor, ProductoProveedorDto>()
                .ForMember(dest => dest.NombreProveedor, opt => opt.MapFrom(src => src.Proveedor.NombreProveedor))
                .ReverseMap();

            // Los ProductoDto ahora se manejan manualmente en el repositorio debido a la complejidad
            // de la relaci�n con m�ltiples proveedores
            CreateMap<Producto, ProductoCrearDto>().ReverseMap();
            CreateMap<Producto, ProductoActualizarDto>().ReverseMap();

            CreateMap<Estado, EstadoDto>().ReverseMap();
            CreateMap<Estado, EstadoCrearDto>().ReverseMap();
            CreateMap<Estado, EstadoActualizarDto>().ReverseMap();

            CreateMap<Almacen, AlmacenDto>().ReverseMap();
            CreateMap<Almacen, AlmacenCrearDto>().ReverseMap();
            CreateMap<Almacen, AlmacenActualizarDto>().ReverseMap();

            // Mappers para Pedidos
            CreateMap<Pedido, PedidoDto>()
                .ForMember(dest => dest.NombreProveedor, opt => opt.MapFrom(src => src.Proveedor.NombreProveedor))
                .ForMember(dest => dest.NombreEstado, opt => opt.MapFrom(src => src.Estado.NombreEstado))
                .ForMember(dest => dest.CodigoEstado, opt => opt.MapFrom(src => src.Estado.CodigoEstado))
                .ForMember(dest => dest.PedidoDetalles, opt => opt.MapFrom(src => src.PedidoDetalles))
                .ForMember(dest => dest.PedidoArchivos, opt => opt.MapFrom(src => src.PedidoArchivos))
                .ReverseMap();
            CreateMap<Pedido, PedidoCrearDto>()
                .ForMember(dest => dest.Detalles, opt => opt.MapFrom(src => src.PedidoDetalles))
                .ReverseMap()
                .ForMember(dest => dest.PedidoDetalles, opt => opt.MapFrom(src => src.Detalles));
            CreateMap<Pedido, PedidoActualizarDto>()
                .ForMember(dest => dest.Detalles, opt => opt.MapFrom(src => src.PedidoDetalles))
                .ReverseMap()
                .ForMember(dest => dest.PedidoDetalles, opt => opt.MapFrom(src => src.Detalles));

            CreateMap<PedidoArchivo, PedidoArchivoDto>().ReverseMap();

            CreateMap<PedidoDetalle, PedidoDetalleDto>()
                .ForMember(dest => dest.NombreProducto, opt => opt.MapFrom(src => src.ProductoProveedor.Producto.NombreProducto))
                .ForMember(dest => dest.PedidoDetalleAlmacenes, opt => opt.MapFrom(src => src.PedidoDetalleAlmacenes))
                .ReverseMap();
            CreateMap<PedidoDetalle, PedidoDetalleCrearDto>().ReverseMap();
            CreateMap<PedidoDetalle, PedidoDetalleActualizarDto>().ReverseMap();

            CreateMap<PedidoDetalleAlmacen, PedidoDetalleAlmacenDto>()
                .ForMember(dest => dest.NombreAlmacen, opt => opt.MapFrom(src => src.Almacen.NombreAlmacen))
                .ReverseMap();
            CreateMap<PedidoDetalleAlmacen, PedidoDetalleAlmacenCrearDto>().ReverseMap();
            CreateMap<PedidoDetalleAlmacen, PedidoDetalleAlmacenActualizarDto>().ReverseMap();
        }
    }
}
