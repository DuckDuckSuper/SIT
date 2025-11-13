

namespace MapProvider.Process
{
    public class GridGenerator
    {
        private const double EarthRadiusMeters = 6378137.0; // WGS84

        public double CalculateCellSize(double vesselLengthMeters)
        {
            double effectiveCellMeters = vesselLengthMeters + 50;
            double cellSizeDegrees = effectiveCellMeters / 111320.0;
            if (cellSizeDegrees < 0.0001) cellSizeDegrees = 0.0001;
            if (cellSizeDegrees > 0.01) cellSizeDegrees = 0.01;
            return cellSizeDegrees;
        }

        public List<Grid> GenerateCircularGrid(double originLat, double originLon,
                                               double destinationLat, double destinationLon,
                                               double vesselLengthMeters)
        {
            var grids = new List<Grid>();
            double centerLat = (originLat + destinationLat) / 2.0;
            double centerLon = (originLon + destinationLon) / 2.0;
            double bigRadiusMeters = HaversineDistance(originLat, originLon, destinationLat, destinationLon);
            double smallRadiusMeters = vesselLengthMeters + 50;
            double smallRadiusDegreesLat = smallRadiusMeters / 111320.0;
            double smallRadiusDegreesLon = smallRadiusMeters / (111320.0 * Math.Cos(centerLat * Math.PI / 180.0));

            int idCounter = 1;
            int ringIndex = 0;

            for (double r = 0; r <= bigRadiusMeters; r += smallRadiusMeters * 2)
            {
                int pointsOnRing = (int)(2 * Math.PI * r / (2 * smallRadiusMeters));
                pointsOnRing = Math.Max(pointsOnRing, 1);

                for (int i = 0; i < pointsOnRing; i++)
                {
                    double angle = 2 * Math.PI * i / pointsOnRing;
                    double dx = r * Math.Cos(angle);
                    double dy = r * Math.Sin(angle);

                    double latOffset = dy / 111320.0;
                    double lonOffset = dx / (111320.0 * Math.Cos(centerLat * Math.PI / 180.0));

                    double cellLat = centerLat + latOffset;
                    double cellLon = centerLon + lonOffset;

                    double distanceFromCenter = HaversineDistance(centerLat, centerLon, cellLat, cellLon);
                    if (distanceFromCenter <= bigRadiusMeters)
                    {
                        grids.Add(new Grid(ringIndex, i, Math.Max(smallRadiusDegreesLat, smallRadiusDegreesLon))
                        {
                            Id = idCounter++,
                            Position = new Coordinate(cellLat, cellLon),
                            DistanceValue = 0,
                            RiskValue = 0,
                            EfficiencyValue = 0,
                            OverallValue = 0,
                            Valid = true
                        });
                    }
                }
                ringIndex++;
            }
            return grids;
        }

        private double HaversineDistance(double lat1, double lon1, double lat2, double lon2)
        {
            double dLat = ToRadians(lat2 - lat1);
            double dLon = ToRadians(lon2 - lon1);
            double a = Math.Pow(Math.Sin(dLat / 2), 2) +
                       Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                       Math.Pow(Math.Sin(dLon / 2), 2);
            double c = 1.2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return EarthRadiusMeters * c;
        }

        private double ToRadians(double deg) => deg * Math.PI / 180.0;

        private bool IsPointInPolygon(Coordinate point, List<Coordinate> polygon)
        {
            bool inside = false;
            int n = polygon.Count;
            for (int i = 0, j = n - 1; i < n; j = i++)
            {
                double xi = polygon[i].Longitude, yi = polygon[i].Latitude;
                double xj = polygon[j].Longitude, yj = polygon[j].Latitude;

                bool intersect = ((yi > point.Latitude) != (yj > point.Latitude)) &&
                                 (point.Longitude < (xj - xi) * (point.Latitude - yi) / (yj - yi + 1e-12) + xi);
                if (intersect) inside = !inside;
            }
            return inside;
        }

        /// <summary>
        /// Parallel marking grids inside multiple polygons with bounding box optimization
        /// </summary>
        public void MarkGridsInsidePolygons(List<Grid> grids, List<List<Coordinate>> polygons)
        {
            if (grids == null || grids.Count == 0 || polygons == null || polygons.Count == 0)
                return;

            // Precompute bounding boxes for each polygon
            var polygonBounds = polygons.Select(poly =>
            {
                double minLat = double.MaxValue, maxLat = double.MinValue;
                double minLon = double.MaxValue, maxLon = double.MinValue;
                foreach (var c in poly)
                {
                    minLat = Math.Min(minLat, c.Latitude);
                    maxLat = Math.Max(maxLat, c.Latitude);
                    minLon = Math.Min(minLon, c.Longitude);
                    maxLon = Math.Max(maxLon, c.Longitude);
                }
                return new { Poly = poly, MinLat = minLat, MaxLat = maxLat, MinLon = minLon, MaxLon = maxLon };
            }).ToList();

            Parallel.ForEach(grids, grid =>
            {
                if (!grid.Valid) return; // skip already invalid grids

                foreach (var poly in polygonBounds)
                {
                    var pos = grid.Position;

                    // Skip polygons whose bounding box doesn't contain the grid
                    if (pos.Latitude < poly.MinLat || pos.Latitude > poly.MaxLat ||
                        pos.Longitude < poly.MinLon || pos.Longitude > poly.MaxLon)
                        continue;

                    // If inside polygon, mark invalid and stop checking
                    if (IsPointInPolygon(pos, poly.Poly))
                    {
                        grid.Valid = false;
                        break;
                    }
                }
            });
        }
    }
}
