namespace MapProvider.Process
{
    public class CoordinateConverter
    {
        /// <summary>
        /// Converts degrees to radians.
        /// </summary>
        public static double DegreesToRadians(double degrees)
        {
            return degrees * (Math.PI / 180.0);
        }

        /// <summary>
        /// Converts radians to degrees.
        /// </summary>
        public static double RadiansToDegrees(double radians)
        {
            return radians * (180.0 / Math.PI);
        }

        /// <summary>
        /// Converts a DMS coordinate string (e.g., "N 1°15'05\"" or "E103°50'19\"") to decimal degrees.
        /// </summary>
        public static double ConvertDmsToDecimal(string dms)
        {
            if (string.IsNullOrWhiteSpace(dms))
                throw new ArgumentException("Input coordinate cannot be null or empty.");

            dms = dms.Trim().ToUpperInvariant();

            // Extract direction
            char direction = dms[0];
            if ("NSEW".IndexOf(direction) == -1)
                throw new ArgumentException("Coordinate must start with N, S, E, or W.");

            // Remove direction and parse numeric parts
            string numericPart = dms.Substring(1).Trim();

            // Split by degree, minute, and second symbols
            string[] degreeSplit = numericPart.Split('°');
            string[] minuteSplit = degreeSplit[1].Split('\'');

            double degrees = double.Parse(degreeSplit[0], CultureInfo.InvariantCulture);
            double minutes = double.Parse(minuteSplit[0], CultureInfo.InvariantCulture);

            double seconds = 0;
            if (minuteSplit[1].Contains("\""))
                seconds = double.Parse(minuteSplit[1].Replace("\"", "").Trim(), CultureInfo.InvariantCulture);

            // Convert DMS → decimal degrees
            double decimalDegrees = degrees + (minutes / 60.0) + (seconds / 3600.0);

            // Apply sign based on direction
            if (direction == 'S' || direction == 'W')
                decimalDegrees *= -1;

            return decimalDegrees;
        }
    }
}