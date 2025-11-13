namespace MapProvider.Models
{
     public class GeoJsonGeometry
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        // Coordinates may be nested (e.g. Polygon vs MultiPolygon)
        [JsonPropertyName("coordinates")]
        public object Coordinates { get; set; }
    }
}