using Newtonsoft.Json;

namespace ApiVentas.Modelos.Dtos
{
    public class InventarioResultadoDto
    {
        [JsonProperty("items")]
        public List<InventarioResultadoItemDto> Items { get; set; } = new();

        [JsonProperty("totalMonto")]
        public decimal TotalMonto { get; set; }

        [JsonProperty("montoCuadre")]
        public decimal MontoCuadre { get; set; }

        [JsonProperty("totalFinal")]
        public decimal TotalFinal { get; set; }
    }
}
