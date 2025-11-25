namespace Veggerby.Boards.Monopoly;

/// <summary>
/// Defines the types of squares on a Monopoly board.
/// </summary>
public enum SquareType
{
    /// <summary>
    /// Start square - players collect $200 when passing.
    /// </summary>
    Go,

    /// <summary>
    /// Regular property square that can be purchased.
    /// </summary>
    Property,

    /// <summary>
    /// Railroad property square.
    /// </summary>
    Railroad,

    /// <summary>
    /// Utility property square (Electric Company, Water Works).
    /// </summary>
    Utility,

    /// <summary>
    /// Community Chest card square.
    /// </summary>
    CommunityChest,

    /// <summary>
    /// Chance card square.
    /// </summary>
    Chance,

    /// <summary>
    /// Income Tax square - pay $200.
    /// </summary>
    IncomeTax,

    /// <summary>
    /// Luxury Tax square - pay $75.
    /// </summary>
    LuxuryTax,

    /// <summary>
    /// Jail/Just Visiting square.
    /// </summary>
    Jail,

    /// <summary>
    /// Free Parking square - no action.
    /// </summary>
    FreeParking,

    /// <summary>
    /// Go To Jail square - sends player to jail.
    /// </summary>
    GoToJail
}
