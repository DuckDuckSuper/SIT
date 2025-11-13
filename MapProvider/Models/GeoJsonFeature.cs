public class GeoJsonFeature
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("properties")]
    public Dictionary<string, object>? Properties { get; set; } // nullable

    [JsonPropertyName("geometry")]
    public GeoJsonGeometry Geometry { get; set; } = new();
}
