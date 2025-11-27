using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Risk;

/// <summary>
/// Calculates reinforcement armies for a player based on territories owned and continent control.
/// </summary>
public static class ReinforcementCalculator
{
    /// <summary>
    /// Calculates the total reinforcements for a player.
    /// </summary>
    /// <param name="player">The player to calculate reinforcements for.</param>
    /// <param name="state">Current game state.</param>
    /// <param name="continents">Continent definitions for bonus calculation.</param>
    /// <returns>Total reinforcement armies (minimum 3).</returns>
    public static int Calculate(Player player, GameState state, IReadOnlyList<Continent> continents)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(continents);

        var ownedTerritories = GetOwnedTerritoryIds(player, state);
        var baseArmies = Math.Max(3, ownedTerritories.Count / 3);
        var continentBonus = CalculateContinentBonus(player, ownedTerritories, continents);

        return baseArmies + continentBonus;
    }

    /// <summary>
    /// Gets all territory IDs owned by a player.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="state">Current game state.</param>
    /// <returns>Set of owned territory IDs.</returns>
    public static HashSet<string> GetOwnedTerritoryIds(Player player, GameState state)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(state);

        var result = new HashSet<string>(StringComparer.Ordinal);

        foreach (var ts in state.GetStates<TerritoryState>())
        {
            if (ts.Owner.Equals(player))
            {
                result.Add(ts.Territory.Id);
            }
        }

        return result;
    }

    /// <summary>
    /// Counts the number of territories owned by a player.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="state">Current game state.</param>
    /// <returns>Number of owned territories.</returns>
    public static int CountOwnedTerritories(Player player, GameState state)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(state);

        var count = 0;

        foreach (var ts in state.GetStates<TerritoryState>())
        {
            if (ts.Owner.Equals(player))
            {
                count++;
            }
        }

        return count;
    }

    /// <summary>
    /// Checks if a player controls all territories in a continent.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="ownedTerritoryIds">Set of territory IDs owned by the player.</param>
    /// <param name="continent">The continent to check.</param>
    /// <returns>True if the player controls the entire continent.</returns>
    public static bool ControlsContinent(Player player, HashSet<string> ownedTerritoryIds, Continent continent)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(ownedTerritoryIds);
        ArgumentNullException.ThrowIfNull(continent);

        foreach (var territoryId in continent.TerritoryIds)
        {
            if (!ownedTerritoryIds.Contains(territoryId))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Calculates the total continent bonus for a player.
    /// </summary>
    private static int CalculateContinentBonus(Player player, HashSet<string> ownedTerritoryIds, IReadOnlyList<Continent> continents)
    {
        var bonus = 0;

        for (int i = 0; i < continents.Count; i++)
        {
            if (ControlsContinent(player, ownedTerritoryIds, continents[i]))
            {
                bonus += continents[i].BonusArmies;
            }
        }

        return bonus;
    }
}
