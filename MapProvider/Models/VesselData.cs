namespace MapProvider.Models
{
    public class VesselData
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public Coordinate Position { get; set; }
        public double Speed { get; set; }
        public double Heading { get; set; }
    }
}