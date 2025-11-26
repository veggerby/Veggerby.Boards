namespace Veggerby.Boards.Monopoly.Cards;

/// <summary>
/// Defines the effect types for Monopoly cards.
/// </summary>
public enum MonopolyCardEffect
{
    /// <summary>
    /// Move to a specific position on the board.
    /// </summary>
    MoveToPosition,

    /// <summary>
    /// Advance to a specific position, collecting Go if passing.
    /// </summary>
    AdvanceToPosition,

    /// <summary>
    /// Move forward a specific number of spaces.
    /// </summary>
    MoveForward,

    /// <summary>
    /// Move backward a specific number of spaces.
    /// </summary>
    MoveBackward,

    /// <summary>
    /// Collect money from the bank.
    /// </summary>
    CollectFromBank,

    /// <summary>
    /// Pay money to the bank.
    /// </summary>
    PayToBank,

    /// <summary>
    /// Collect money from all other players.
    /// </summary>
    CollectFromPlayers,

    /// <summary>
    /// Pay money to all other players.
    /// </summary>
    PayToPlayers,

    /// <summary>
    /// Go directly to jail (do not pass Go, do not collect $200).
    /// </summary>
    GoToJail,

    /// <summary>
    /// Get out of jail free card.
    /// </summary>
    GetOutOfJailFree,

    /// <summary>
    /// Advance to the nearest railroad, pay double rent if owned.
    /// </summary>
    AdvanceToNearestRailroad,

    /// <summary>
    /// Advance to the nearest utility, pay 10x dice roll if owned.
    /// </summary>
    AdvanceToNearestUtility,

    /// <summary>
    /// Pay for property repairs (houses and hotels) - deferred in base implementation.
    /// </summary>
    PropertyRepairs
}
