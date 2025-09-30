namespace Veggerby.Boards;

/// <summary>
/// Provides constant values for canonical direction identifiers used in board movement patterns and tile relations.
/// </summary>
public static class Constants
{
    /// <summary>
    /// Canonical direction identifiers (orthogonal + diagonal) used for movement patterns and tile relations.
    /// </summary>
    public static class Directions
    {
        /// <summary>
        /// Left direction (e.g., decreasing file or column).
        /// </summary>
        public const string Left = "left";
        /// <summary>
        /// Right direction (e.g., increasing file or column).
        /// </summary>
        public const string Right = "right";
        /// <summary>
        /// Up direction (e.g., increasing row index in some coordinate systems).
        /// </summary>
        public const string Up = "up";
        /// <summary>
        /// Down direction (e.g., decreasing row index in some coordinate systems).
        /// </summary>
        public const string Down = "down";
        /// <summary>
        /// Across / lateral direction.
        /// </summary>
        public const string Across = "across";

        /// <summary>North (increasing rank for white perspective).</summary>
        public const string North = "north";
        /// <summary>South (decreasing rank for white perspective).</summary>
        public const string South = "south";
        /// <summary>East (increasing file).</summary>
        public const string East = "east";
        /// <summary>West (decreasing file).</summary>
        public const string West = "west";
        /// <summary>North-East diagonal.</summary>
        public const string NorthEast = "north-east";
        /// <summary>North-West diagonal.</summary>
        public const string NorthWest = "north-west";
        /// <summary>South-East diagonal.</summary>
        public const string SouthEast = "south-east";
        /// <summary>South-West diagonal.</summary>
        public const string SouthWest = "south-west";

        /// <summary>Clockwise rotation direction.</summary>
        public const string Clockwise = "clockwise";
        /// <summary>Counter-clockwise rotation direction.</summary>
        public const string CounterClockwise = "counter-clockwise";
    }
}