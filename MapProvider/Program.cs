// Convert DMS to decimal
        double originLatitude = CoordinateConverter.ConvertDmsToDecimal("N 1°16'12\"");
        double originLongitude = CoordinateConverter.ConvertDmsToDecimal("E103°52'00\"");
        double destinationLatitude = CoordinateConverter.ConvertDmsToDecimal("N 1°14'44\"");
        double destinationLongitude = CoordinateConverter.ConvertDmsToDecimal("E103°58'12\"");

        var generator = new GridGenerator();
        double vesselLengthMeters = 15; // vessel length in meters

        // Generate grids
        List<Grid> grids = generator.GenerateCircularGrid(
            originLatitude, originLongitude,
            destinationLatitude, destinationLongitude,
            vesselLengthMeters
        );

        // Read GeoJSON
        var reader = new GeoJsonReader();
        var geoData = reader.ReadGeoJson("singapore.geojson");

        // Prepare polygons
        var polygons = new List<List<Coordinate>>();

        if (geoData != null && geoData.Features.Count > 0)
        {
            foreach (var feature in geoData.Features)
            {
                var polygonCoords = reader.GetPolygonCoordinates(feature);
                polygons.Add(polygonCoords);

                if (feature.Properties != null && feature.Properties.ContainsKey("name"))
                    Console.WriteLine($"Feature: {feature.Properties["name"]}");
                else
                    Console.WriteLine("Feature has no name property");

                Console.WriteLine($"Polygon has {polygonCoords.Count} coordinates");
            }
        }

        // Mark grids inside polygons
        generator.MarkGridsInsidePolygons(grids, polygons);

        // Export to JSON
        string json = JsonSerializer.Serialize(grids, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText("grids.json", json);
        Console.WriteLine("Exported grid data to grids.json");
        Console.WriteLine($"Generated {grids.Count} grid cells in ROI.");