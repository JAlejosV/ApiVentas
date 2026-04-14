using Newtonsoft.Json;

namespace ApiVentas.Modelos.Dtos
{
    public class InventarioResultadoItemDto
    {
        [JsonProperty("codigo")]
        public string CodigoBarras { get; set; }

        [JsonProperty("producto")]
        public string Articulo { get; set; }

        [JsonProperty("totalFisico")]
        public decimal TotalFisico { get; set; }

        [JsonProperty("totalSistema")]
        public decimal TotalSistema { get; set; }

        [JsonProperty("diferencia")]
        public decimal Diferencia { get; set; }

        [JsonProperty("precio")]
        public decimal Precio { get; set; }

        [JsonProperty("monto")]
        public decimal Monto { get; set; }
    }
}
