namespace ApiVentas.Modelos.Dtos
{
    public class PermisoDto
    {
        public int IdPermiso { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string ModuloNombre { get; set; }
        public int IdModulo { get; set; }
    }
}