
using System.Text.Json.Serialization;

namespace MapProvider.Models
{
    public class AISData
    {
        [JsonPropertyName("MessageType")]
        public string MessageType { get; set; }

        [JsonPropertyName("Message")]
        public AISMessage Message { get; set; }

        [JsonPropertyName("MetaData")]
        public AISMetaData MetaData { get; set; }
    }

    public class AISMessage
    {
        [JsonPropertyName("PositionReport")]
        public PositionReport PositionReport { get; set; }

        [JsonPropertyName("ShipStaticData")]
        public ShipStaticData ShipStaticData { get; set; }
    }

    // ===== Vessel (Static Details) =====
    public class Vessel
    {
        public long MMSI { get; set; }
        public string Name { get; set; }
        public string CallSign { get; set; }
        public string Destination { get; set; }
        public Dimension Dimension { get; set; }
        public double MaximumStaticDraught { get; set; }
        public long IMONumber { get; set; }
        public int Type { get; set; }
        public ETA Eta { get; set; }
    }

    public class Dimension
    {
        public int A { get; set; }
        public int B { get; set; }
        public int C { get; set; }
        public int D { get; set; }
    }

    public class ETA
    {
        public int Day { get; set; }
        public int Hour { get; set; }
        public int Minute { get; set; }
        public int Month { get; set; }
    }

    // ===== Position (Dynamic Data) =====
    public class Position
    {
        public long MMSI { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double COG { get; set; }
        public double SOG { get; set; }
        public int TrueHeading { get; set; }
        public int RateOfTurn { get; set; }
        public int NavigationalStatus { get; set; }
        public bool PositionAccuracy { get; set; }
        public bool RAIM { get; set; }
        public bool Valid { get; set; }
        public int Timestamp { get; set; }
    }

    // ===== Original AIS JSON structures =====
    public class PositionReport
    {
         [JsonPropertyName("Cog")]
        public double COG { get; set; }
        public int CommunicationState { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int MessageID { get; set; }
        public int NavigationalStatus { get; set; }
        public bool PositionAccuracy { get; set; }
        public bool RAIM { get; set; }
        public int RateOfTurn { get; set; }
        public int RepeatIndicator { get; set; }
         [JsonPropertyName("Sog")]
        public double SOG { get; set; }
        public int Spare { get; set; }
        public int SpecialManoeuvreIndicator { get; set; }
        public int Timestamp { get; set; }
        public int TrueHeading { get; set; }
        public long UserID { get; set; }
        public bool Valid { get; set; }
    }

    public class ShipStaticData
    {
        public int AISVersion { get; set; }
        public string CallSign { get; set; }
        public string Destination { get; set; }
        public Dimension Dimension { get; set; }
        public bool DTE { get; set; }
        public ETA Eta { get; set; }
        public int FixType { get; set; }
        public long IMONumber { get; set; }
        public double MaximumStaticDraught { get; set; }
        public int MessageID { get; set; }
        public string Name { get; set; }
        public int RepeatIndicator { get; set; }
        public bool Spare { get; set; }
        public int Type { get; set; }
        public long UserID { get; set; }
        public bool Valid { get; set; }
    }

    public class AISMetaData
    {
        public long MMSI { get; set; }

        [JsonPropertyName("MMSI_String")]
        public long MMSI_String { get; set; }

        [JsonPropertyName("ShipName")]
        public string ShipName { get; set; }

        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }

        [JsonPropertyName("time_utc")]
        public string TimeUTC { get; set; }
    }
    
}
