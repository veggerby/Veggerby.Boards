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
    /// Creates a new MonopolySquareInfo.
    /// </summary>
    public MonopolySquareInfo(int position, string name, SquareType squareType, PropertyColorGroup colorGroup = PropertyColorGroup.None, int price = 0, int baseRent = 0)
    {
        ArgumentNullException.ThrowIfNull(name);

        Position = position;
        Name = name;
        SquareType = squareType;
        ColorGroup = colorGroup;
        Price = price;
        BaseRent = baseRent;
    }

    /// <summary>
    /// Gets whether this square can be purchased.
    /// </summary>
    public bool IsPurchasable => SquareType is SquareType.Property or SquareType.Railroad or SquareType.Utility;
}
