using System.Collections.Generic;

namespace Veggerby.Boards.Risk;

/// <summary>
/// Immutable extras state containing Risk-specific game configuration and phase tracking.
/// </summary>
/// <remarks>
/// This record is stored in GameState and contains continent definitions and current phase information.
/// </remarks>
/// <param name="Continents">The collection of continent definitions for the game map.</param>
/// <param name="CurrentPhase">The current game phase (Reinforce, Attack, Fortify).</param>
/// <param name="ReinforcementsRemaining">The number of reinforcement armies remaining to place for the active player.</param>
/// <param name="ConqueredThisTurn">Whether the active player has conquered at least one territory this turn (earns a card).</param>
/// <param name="MinimumConquestArmies">The minimum armies that must be moved into a conquered territory (equals attacker dice count).</param>
public sealed record RiskStateExtras(
    IReadOnlyList<Continent> Continents,
    RiskPhase CurrentPhase,
    int ReinforcementsRemaining,
    bool ConqueredThisTurn,
    int? MinimumConquestArmies);

/// <summary>
/// Represents the phases of a Risk turn.
/// </summary>
public enum RiskPhase
{
    /// <summary>
    /// Reinforcement phase: player places new armies on owned territories.
    /// </summary>
    Reinforce,

    /// <summary>
    /// Attack phase: player may attack adjacent enemy territories.
    /// </summary>
    Attack,

    /// <summary>
    /// Fortify phase: player may move armies between connected owned territories.
    /// </summary>
    Fortify
}
