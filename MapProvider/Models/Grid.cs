namespace MapProvider.Models
{
    public class Grid
    {
        public int Id { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }

        public bool Valid { get; set; }=true;

        public Coordinate Position { get; set; }

        /// <summary>
        /// the higher distance value, the faster the vessel can reach to the destination
        /// </summary>
        public int DistanceValue { get; set; }

        /// <summary>
        /// the higher risk value, the riskier the cell is
        /// </summary>
        public int RiskValue { get; set; }

        /// <summary>
        /// the higher efficiency value (push from the current or wind), the more efficient the route is
        /// </summary>
        public int EfficiencyValue { get; set; }

        /// <summary>
        /// the overall value combining distance, risk, and efficiency
        /// it should be (distance + efficency) - risk
        /// </summary>
        public int OverallValue { get; set; }
        public double CellSize { get; set; }

        public Grid(int rows, int columns, double cellSize)
        {
            Row = rows;
            Column = columns;
            CellSize = cellSize;
        }
    }
}