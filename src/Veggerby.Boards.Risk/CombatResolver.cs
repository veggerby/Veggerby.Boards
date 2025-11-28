using System;

using Veggerby.Boards.Random;

namespace Veggerby.Boards.Risk;

/// <summary>
/// Represents the result of a combat resolution.
/// </summary>
public readonly record struct CombatResult
{
    /// <summary>
    /// Gets the number of armies the attacker lost.
    /// </summary>
    public int AttackerLosses
    {
        get; init;
    }

    /// <summary>
    /// Gets the number of armies the defender lost.
    /// </summary>
    public int DefenderLosses
    {
        get; init;
    }

    /// <summary>
    /// Gets the dice values rolled by the attacker (sorted descending).
    /// </summary>
    public int[] AttackerRolls
    {
        get; init;
    }

    /// <summary>
    /// Gets the dice values rolled by the defender (sorted descending).
    /// </summary>
    public int[] DefenderRolls
    {
        get; init;
    }

    /// <summary>
    /// Gets whether the defender was eliminated (all armies lost).
    /// </summary>
    public bool DefenderEliminated
    {
        get; init;
    }
}

/// <summary>
/// Resolves combat between attacker and defender using the standard Risk dice comparison rules.
/// </summary>
/// <remarks>
/// Combat uses deterministic dice rolling via IRandomSource. Dice are sorted descending and compared pairwise.
/// Defender wins ties.
/// </remarks>
public static class CombatResolver
{
    /// <summary>
    /// Resolves combat between attacker and defender.
    /// </summary>
    /// <param name="attackerDiceCount">Number of attacker dice (1-3).</param>
    /// <param name="defenderArmies">Number of defender armies (determines defender dice: 1-2).</param>
    /// <param name="random">Deterministic random source.</param>
    /// <returns>Combat result including losses and dice values.</returns>
    public static CombatResult Resolve(int attackerDiceCount, int defenderArmies, IRandomSource random)
    {
        ArgumentNullException.ThrowIfNull(random);

        if (attackerDiceCount < 1 || attackerDiceCount > 3)
        {
            throw new ArgumentOutOfRangeException(nameof(attackerDiceCount), "Attacker must roll 1-3 dice.");
        }

        if (defenderArmies < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(defenderArmies), "Defender must have at least 1 army.");
        }

        // Defender rolls 1-2 dice based on army count
        var defenderDiceCount = Math.Min(2, defenderArmies);

        // Roll attacker dice
        var attackerRolls = new int[attackerDiceCount];

        for (int i = 0; i < attackerDiceCount; i++)
        {
            attackerRolls[i] = RollDie(random);
        }

        // Roll defender dice
        var defenderRolls = new int[defenderDiceCount];

        for (int i = 0; i < defenderDiceCount; i++)
        {
            defenderRolls[i] = RollDie(random);
        }

        // Sort descending (in-place)
        SortDescending(attackerRolls);
        SortDescending(defenderRolls);

        // Compare pairs: highest vs highest, second vs second
        // Defender wins ties
        var attackerLosses = 0;
        var defenderLosses = 0;
        var pairs = Math.Min(attackerRolls.Length, defenderRolls.Length);

        for (int i = 0; i < pairs; i++)
        {
            if (attackerRolls[i] > defenderRolls[i])
            {
                defenderLosses++;
            }
            else
            {
                // Defender wins ties
                attackerLosses++;
            }
        }

        return new CombatResult
        {
            AttackerLosses = attackerLosses,
            DefenderLosses = defenderLosses,
            AttackerRolls = attackerRolls,
            DefenderRolls = defenderRolls,
            DefenderEliminated = defenderLosses >= defenderArmies
        };
    }

    /// <summary>
    /// Resolves combat with pre-rolled dice values (for deterministic testing).
    /// </summary>
    /// <param name="attackerRolls">Pre-rolled attacker dice (will be sorted).</param>
    /// <param name="defenderRolls">Pre-rolled defender dice (will be sorted).</param>
    /// <param name="defenderArmies">Total defender armies (to determine elimination).</param>
    /// <returns>Combat result.</returns>
    public static CombatResult ResolveWithRolls(int[] attackerRolls, int[] defenderRolls, int defenderArmies)
    {
        ArgumentNullException.ThrowIfNull(attackerRolls);
        ArgumentNullException.ThrowIfNull(defenderRolls);

        if (attackerRolls.Length == 0 || attackerRolls.Length > 3)
        {
            throw new ArgumentOutOfRangeException(nameof(attackerRolls), "Attacker must have 1-3 dice.");
        }

        if (defenderRolls.Length == 0 || defenderRolls.Length > 2)
        {
            throw new ArgumentOutOfRangeException(nameof(defenderRolls), "Defender must have 1-2 dice.");
        }

        // Copy to avoid mutating input
        var aSorted = new int[attackerRolls.Length];
        Array.Copy(attackerRolls, aSorted, attackerRolls.Length);
        var dSorted = new int[defenderRolls.Length];
        Array.Copy(defenderRolls, dSorted, defenderRolls.Length);

        SortDescending(aSorted);
        SortDescending(dSorted);

        var attackerLosses = 0;
        var defenderLosses = 0;
        var pairs = Math.Min(aSorted.Length, dSorted.Length);

        for (int i = 0; i < pairs; i++)
        {
            if (aSorted[i] > dSorted[i])
            {
                defenderLosses++;
            }
            else
            {
                attackerLosses++;
            }
        }

        return new CombatResult
        {
            AttackerLosses = attackerLosses,
            DefenderLosses = defenderLosses,
            AttackerRolls = aSorted,
            DefenderRolls = dSorted,
            DefenderEliminated = defenderLosses >= defenderArmies
        };
    }

    /// <summary>
    /// Rolls a single 6-sided die using the random source.
    /// </summary>
    private static int RollDie(IRandomSource random)
    {
        // NextUInt gives uint; convert to 1-6 range
        var value = random.NextUInt();
        return (int)(value % 6) + 1;
    }

    /// <summary>
    /// Sorts an array in descending order (in-place, insertion sort for small arrays).
    /// </summary>
    private static void SortDescending(int[] arr)
    {
        for (int i = 1; i < arr.Length; i++)
        {
            var key = arr[i];
            var j = i - 1;

            while (j >= 0 && arr[j] < key)
            {
                arr[j + 1] = arr[j];
                j--;
            }

            arr[j + 1] = key;
        }
    }
}
