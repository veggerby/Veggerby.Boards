using System;
using System.Collections.Generic;
using System.Linq;

namespace Veggerby.Boards.Monopoly;

/// <summary>
/// Static configuration defining the standard Monopoly board layout.
/// </summary>
public static class MonopolyBoardConfiguration
{
    /// <summary>
    /// Gets all 40 board squares in position order.
    /// </summary>
    public static IReadOnlyList<MonopolySquareInfo> Squares { get; } = CreateSquares();

    /// <summary>
    /// Gets squares by color group.
    /// </summary>
    public static IReadOnlyDictionary<PropertyColorGroup, IReadOnlyList<MonopolySquareInfo>> SquaresByColorGroup { get; } = CreateSquaresByColorGroup();

    private static IReadOnlyList<MonopolySquareInfo> CreateSquares()
    {
        return new List<MonopolySquareInfo>
        {
            // Row 1 (bottom row, positions 0-10)
            new(0, "Go", SquareType.Go),
            new(1, "Mediterranean Avenue", SquareType.Property, PropertyColorGroup.Brown, 60, 2),
            new(2, "Community Chest 1", SquareType.CommunityChest),
            new(3, "Baltic Avenue", SquareType.Property, PropertyColorGroup.Brown, 60, 4),
            new(4, "Income Tax", SquareType.IncomeTax),
            new(5, "Reading Railroad", SquareType.Railroad, PropertyColorGroup.Railroad, 200, 25),
            new(6, "Oriental Avenue", SquareType.Property, PropertyColorGroup.LightBlue, 100, 6),
            new(7, "Chance 1", SquareType.Chance),
            new(8, "Vermont Avenue", SquareType.Property, PropertyColorGroup.LightBlue, 100, 6),
            new(9, "Connecticut Avenue", SquareType.Property, PropertyColorGroup.LightBlue, 120, 8),
            new(10, "Jail", SquareType.Jail),

            // Row 2 (left column, positions 11-19)
            new(11, "St. Charles Place", SquareType.Property, PropertyColorGroup.Pink, 140, 10),
            new(12, "Electric Company", SquareType.Utility, PropertyColorGroup.Utility, 150, 0),
            new(13, "States Avenue", SquareType.Property, PropertyColorGroup.Pink, 140, 10),
            new(14, "Virginia Avenue", SquareType.Property, PropertyColorGroup.Pink, 160, 12),
            new(15, "Pennsylvania Railroad", SquareType.Railroad, PropertyColorGroup.Railroad, 200, 25),
            new(16, "St. James Place", SquareType.Property, PropertyColorGroup.Orange, 180, 14),
            new(17, "Community Chest 2", SquareType.CommunityChest),
            new(18, "Tennessee Avenue", SquareType.Property, PropertyColorGroup.Orange, 180, 14),
            new(19, "New York Avenue", SquareType.Property, PropertyColorGroup.Orange, 200, 16),
            new(20, "Free Parking", SquareType.FreeParking),

            // Row 3 (top row, positions 21-29)
            new(21, "Kentucky Avenue", SquareType.Property, PropertyColorGroup.Red, 220, 18),
            new(22, "Chance 2", SquareType.Chance),
            new(23, "Indiana Avenue", SquareType.Property, PropertyColorGroup.Red, 220, 18),
            new(24, "Illinois Avenue", SquareType.Property, PropertyColorGroup.Red, 240, 20),
            new(25, "B&O Railroad", SquareType.Railroad, PropertyColorGroup.Railroad, 200, 25),
            new(26, "Atlantic Avenue", SquareType.Property, PropertyColorGroup.Yellow, 260, 22),
            new(27, "Ventnor Avenue", SquareType.Property, PropertyColorGroup.Yellow, 260, 22),
            new(28, "Water Works", SquareType.Utility, PropertyColorGroup.Utility, 150, 0),
            new(29, "Marvin Gardens", SquareType.Property, PropertyColorGroup.Yellow, 280, 24),
            new(30, "Go To Jail", SquareType.GoToJail),

            // Row 4 (right column, positions 31-39)
            new(31, "Pacific Avenue", SquareType.Property, PropertyColorGroup.Green, 300, 26),
            new(32, "North Carolina Avenue", SquareType.Property, PropertyColorGroup.Green, 300, 26),
            new(33, "Community Chest 3", SquareType.CommunityChest),
            new(34, "Pennsylvania Avenue", SquareType.Property, PropertyColorGroup.Green, 320, 28),
            new(35, "Short Line Railroad", SquareType.Railroad, PropertyColorGroup.Railroad, 200, 25),
            new(36, "Chance 3", SquareType.Chance),
            new(37, "Park Place", SquareType.Property, PropertyColorGroup.DarkBlue, 350, 35),
            new(38, "Luxury Tax", SquareType.LuxuryTax),
            new(39, "Boardwalk", SquareType.Property, PropertyColorGroup.DarkBlue, 400, 50)
        }.AsReadOnly();
    }

    private static IReadOnlyDictionary<PropertyColorGroup, IReadOnlyList<MonopolySquareInfo>> CreateSquaresByColorGroup()
    {
        return Squares
            .Where(s => s.ColorGroup != PropertyColorGroup.None)
            .GroupBy(s => s.ColorGroup)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyList<MonopolySquareInfo>)g.ToList().AsReadOnly());
    }

    /// <summary>
    /// Gets the count of properties in a color group.
    /// </summary>
    public static int GetColorGroupCount(PropertyColorGroup colorGroup)
    {
        return SquaresByColorGroup.TryGetValue(colorGroup, out var squares) ? squares.Count : 0;
    }

    /// <summary>
    /// Gets a square by position.
    /// </summary>
    public static MonopolySquareInfo GetSquare(int position)
    {
        if (position < 0 || position >= 40)
        {
            throw new ArgumentOutOfRangeException(nameof(position), "Position must be between 0 and 39");
        }

        return Squares[position];
    }

    /// <summary>
    /// Gets the tile ID for a position.
    /// </summary>
    public static string GetTileId(int position)
    {
        return $"square-{position}";
    }

    /// <summary>
    /// Gets the position from a tile ID.
    /// </summary>
    public static int GetPosition(string tileId)
    {
        ArgumentNullException.ThrowIfNull(tileId);

        if (!tileId.StartsWith("square-", StringComparison.Ordinal))
        {
            throw new ArgumentException($"Invalid tile ID: {tileId}", nameof(tileId));
        }

        return int.Parse(tileId.AsSpan(7));
    }
}
