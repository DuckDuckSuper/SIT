namespace MapProvider.Models
{
    public class GeoJsonFeatureCollection
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("features")]
        public List<GeoJsonFeature> Features { get; set; } = new();
    }
}