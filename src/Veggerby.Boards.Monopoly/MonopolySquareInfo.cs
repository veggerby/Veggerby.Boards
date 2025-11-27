using System;

namespace Veggerby.Boards.Monopoly;

/// <summary>
/// Immutable configuration for a Monopoly board square.
/// </summary>
public sealed record MonopolySquareInfo
{
    /// <summary>
    /// Gets the position on the board (0-39).
    /// </summary>
    public int Position
    {
        get; init;
    }

    /// <summary>
    /// Gets the square name (e.g., "Mediterranean Avenue").
    /// </summary>
    public string Name
    {
        get; init;
    }

    /// <summary>
    /// Gets the type of square.
    /// </summary>
    public SquareType SquareType
    {
        get; init;
    }

    /// <summary>
    /// Gets the color group for property squares.
    /// </summary>
    public PropertyColorGroup ColorGroup
    {
        get; init;
    }

    /// <summary>
    /// Gets the purchase price for purchasable squares.
    /// </summary>
    public int Price
    {
        get; init;
    }

    /// <summary>
    /// Gets the base rent for property squares.
    /// </summary>
    public int BaseRent
    {
        get; init;
    }

    /// <summary>
    /// Gets the cost to build a house on this property.
    /// </summary>
    public int HouseCost
    {
        get; init;
    }

    /// <summary>
    /// Gets the rent schedule for properties (rent with 1, 2, 3, 4 houses and hotel).
    /// Index 0 = 1 house, Index 1 = 2 houses, Index 2 = 3 houses, Index 3 = 4 houses, Index 4 = hotel.
    /// </summary>
    public int[] RentSchedule
    {
        get; init;
    }

    /// <summary>
    /// Creates a new MonopolySquareInfo.
    /// </summary>
    public MonopolySquareInfo(
        int position,
        string name,
        SquareType squareType,
        PropertyColorGroup colorGroup = PropertyColorGroup.None,
        int price = 0,
        int baseRent = 0,
        int houseCost = 0,
        int[]? rentSchedule = null)
    {
        ArgumentNullException.ThrowIfNull(name);

        Position = position;
        Name = name;
        SquareType = squareType;
        ColorGroup = colorGroup;
        Price = price;
        BaseRent = baseRent;
        HouseCost = houseCost;
        RentSchedule = rentSchedule ?? Array.Empty<int>();
    }

    /// <summary>
    /// Gets whether this square can be purchased.
    /// </summary>
    public bool IsPurchasable => SquareType is SquareType.Property or SquareType.Railroad or SquareType.Utility;

    /// <summary>
    /// Gets whether this square can have houses/hotels built on it.
    /// </summary>
    public bool CanBuildHouses => SquareType == SquareType.Property && HouseCost > 0;

    /// <summary>
    /// Gets the rent for the given house count.
    /// </summary>
    /// <param name="houseCount">Number of houses (0-4) or 5 for hotel.</param>
    /// <returns>The rent amount.</returns>
    public int GetRentForHouseCount(int houseCount)
    {
        if (houseCount <= 0)
        {
            return BaseRent;
        }

        if (RentSchedule.Length == 0)
        {
            return BaseRent;
        }

        // RentSchedule: index 0 = 1 house, index 1 = 2 houses, etc.
        var index = Math.Min(houseCount - 1, RentSchedule.Length - 1);

        return RentSchedule[index];
    }
}
