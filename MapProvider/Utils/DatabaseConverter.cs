namespace MapProvider.Utils
{
    public static class DatabaseConverter
    {   
        public static DateTime ConvertTimestamp(int unixTimestamp)
        {
            // AIS Timestamp is usually in seconds since epoch
            return DateTimeOffset.FromUnixTimeSeconds(unixTimestamp).UtcDateTime;
        }
    }
}