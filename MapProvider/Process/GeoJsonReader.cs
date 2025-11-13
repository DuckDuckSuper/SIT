

namespace MapProvider.Process
{
    public class GeoJsonReader
    {
        /// <summary>
        /// Reads a GeoJSON file and deserializes it into a feature collection
        /// </summary>
        public GeoJsonFeatureCollection? ReadGeoJson(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("GeoJSON file not found", filePath);

            string json = File.ReadAllText(filePath);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            return JsonSerializer.Deserialize<GeoJsonFeatureCollection>(json, options);
        }

        /// <summary>
        /// Extracts coordinates for all features.
        /// Returns dictionary: feature name -> list of coordinates
        /// If no "name" property exists, a default key is generated.
        /// </summary>
        public Dictionary<string, List<Coordinate>> ExtractAllPolygons(GeoJsonFeatureCollection geoData)
        {
            var result = new Dictionary<string, List<Coordinate>>();
            int unnamedCounter = 1;

            foreach (var feature in geoData.Features)
            {
                string featureName = "Unnamed_" + (unnamedCounter++);
                if (feature.Properties != null && feature.Properties.ContainsKey("name"))
                {
                    featureName = feature.Properties["name"]?.ToString() ?? featureName;
                }

                List<Coordinate> coords = GetPolygonCoordinates(feature);
                result[featureName] = coords;
            }

            return result;
        }

        /// <summary>
        /// Extract coordinates as List<Coordinate> from a single feature
        /// Supports Polygon and MultiPolygon
        /// Ignores Z-value (third coordinate)
        /// </summary>
        public List<Coordinate> GetPolygonCoordinates(GeoJsonFeature feature)
        {
            var coordinates = new List<Coordinate>();

            try
            {
                if (feature.Geometry.Type.Equals("Polygon", StringComparison.OrdinalIgnoreCase))
                {
                    var rawCoords = JsonSerializer.Deserialize<List<List<List<double>>>>(
                        feature.Geometry.Coordinates.ToString() ?? "[]"
                    );

                    if (rawCoords != null && rawCoords.Count > 0)
                    {
                        foreach (var pair in rawCoords[0])
                        {
                            if (pair.Count >= 2)
                                coordinates.Add(new Coordinate(pair[1], pair[0])); // lat, lon
                        }
                    }
                }
                else if (feature.Geometry.Type.Equals("MultiPolygon", StringComparison.OrdinalIgnoreCase))
                {
                    var rawCoords = JsonSerializer.Deserialize<List<List<List<List<double>>>>>(
                        feature.Geometry.Coordinates.ToString() ?? "[]"
                    );

                    if (rawCoords != null && rawCoords.Count > 0)
                    {
                        foreach (var poly in rawCoords)
                        {
                            foreach (var ring in poly)
                            {
                                foreach (var pair in ring)
                                {
                                    if (pair.Count >= 2)
                                        coordinates.Add(new Coordinate(pair[1], pair[0])); // lat, lon
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                // Ignore malformed geometry
            }

            return coordinates;
        }
    }
}
