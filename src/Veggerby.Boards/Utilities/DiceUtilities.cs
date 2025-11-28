using System;

using Veggerby.Boards.Random;

namespace Veggerby.Boards.Utilities;

/// <summary>
/// Common dice rolling utilities for deterministic game mechanics.
/// </summary>
/// <remarks>
/// Provides standard dice operations used across multiple game modules (Risk, Monopoly, Ludo, Backgammon).
/// All operations use <see cref="IRandomSource"/> for deterministic replay support.
/// </remarks>
public static class DiceUtilities
{
    /// <summary>
    /// Rolls a single 6-sided die (d6) returning a value between 1 and 6 inclusive.
    /// </summary>
    /// <param name="random">Deterministic random source.</param>
    /// <returns>Value between 1 and 6.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="random"/> is null.</exception>
    public static int RollD6(IRandomSource random)
    {
        ArgumentNullException.ThrowIfNull(random);

        var value = random.NextUInt();
        return (int)(value % 6) + 1;
    }

    /// <summary>
    /// Rolls multiple 6-sided dice (d6) and returns the results.
    /// </summary>
    /// <param name="count">Number of dice to roll (must be positive).</param>
    /// <param name="random">Deterministic random source.</param>
    /// <returns>Array of roll values, each between 1 and 6.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="random"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="count"/> is less than 1.</exception>
    public static int[] RollMultipleD6(int count, IRandomSource random)
    {
        ArgumentNullException.ThrowIfNull(random);

        if (count < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "Dice count must be at least 1.");
        }

        var rolls = new int[count];

        for (int i = 0; i < count; i++)
        {
            rolls[i] = RollD6(random);
        }

        return rolls;
    }

    /// <summary>
    /// Rolls a die with a specified number of sides (dN).
    /// </summary>
    /// <param name="sides">Number of sides on the die (must be at least 2).</param>
    /// <param name="random">Deterministic random source.</param>
    /// <returns>Value between 1 and <paramref name="sides"/> inclusive.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="random"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="sides"/> is less than 2.</exception>
    public static int RollDN(int sides, IRandomSource random)
    {
        ArgumentNullException.ThrowIfNull(random);

        if (sides < 2)
        {
            throw new ArgumentOutOfRangeException(nameof(sides), "Die must have at least 2 sides.");
        }

        var value = random.NextUInt();
        return (int)(value % (uint)sides) + 1;
    }

    /// <summary>
    /// Sorts an array of dice values in descending order (in-place).
    /// </summary>
    /// <param name="values">Array of values to sort.</param>
    /// <remarks>
    /// Uses insertion sort optimized for small arrays (typically 1-3 dice in most games).
    /// This avoids allocation overhead of LINQ or Array.Sort for hot path operations.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is null.</exception>
    public static void SortDescending(int[] values)
    {
        ArgumentNullException.ThrowIfNull(values);

        for (int i = 1; i < values.Length; i++)
        {
            var key = values[i];
            var j = i - 1;

            while (j >= 0 && values[j] < key)
            {
                values[j + 1] = values[j];
                j--;
            }

            values[j + 1] = key;
        }
    }

    /// <summary>
    /// Compares two sorted dice arrays pairwise (highest vs highest, second vs second, etc.)
    /// and returns the number of wins for each side.
    /// </summary>
    /// <param name="attackerDice">Attacker's dice values (should be sorted descending).</param>
    /// <param name="defenderDice">Defender's dice values (should be sorted descending).</param>
    /// <param name="defenderWinsTies">If true, defender wins on ties; otherwise attacker wins ties.</param>
    /// <returns>Tuple of (attacker wins, defender wins).</returns>
    /// <remarks>
    /// This implements the standard Risk-style dice comparison where pairs are compared
    /// from highest to lowest. The number of comparisons equals the minimum of the two array lengths.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when either array is null.</exception>
    public static (int AttackerWins, int DefenderWins) CompareDicePairs(
        int[] attackerDice,
        int[] defenderDice,
        bool defenderWinsTies = true)
    {
        ArgumentNullException.ThrowIfNull(attackerDice);
        ArgumentNullException.ThrowIfNull(defenderDice);

        var attackerWins = 0;
        var defenderWins = 0;
        var pairs = Math.Min(attackerDice.Length, defenderDice.Length);

        for (int i = 0; i < pairs; i++)
        {
            if (attackerDice[i] > defenderDice[i])
            {
                attackerWins++;
            }
            else if (attackerDice[i] < defenderDice[i])
            {
                defenderWins++;
            }
            else
            {
                // Tie
                if (defenderWinsTies)
                {
                    defenderWins++;
                }
                else
                {
                    attackerWins++;
                }
            }
        }

        return (attackerWins, defenderWins);
    }

    /// <summary>
    /// Calculates the sum of all dice values in an array.
    /// </summary>
    /// <param name="values">Array of dice values.</param>
    /// <returns>Sum of all values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is null.</exception>
    public static int Sum(int[] values)
    {
        ArgumentNullException.ThrowIfNull(values);

        var sum = 0;

        for (int i = 0; i < values.Length; i++)
        {
            sum += values[i];
        }

        return sum;
    }

    /// <summary>
    /// Checks if all dice in an array have the same value (doubles, triples, etc.).
    /// </summary>
    /// <param name="values">Array of dice values.</param>
    /// <returns>True if all values are identical; false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is null.</exception>
    public static bool AreAllEqual(int[] values)
    {
        ArgumentNullException.ThrowIfNull(values);

        if (values.Length <= 1)
        {
            return true;
        }

        var first = values[0];

        for (int i = 1; i < values.Length; i++)
        {
            if (values[i] != first)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Gets the highest value from an array of dice values.
    /// </summary>
    /// <param name="values">Array of dice values.</param>
    /// <returns>Highest value in the array.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="values"/> is empty.</exception>
    public static int Max(int[] values)
    {
        ArgumentNullException.ThrowIfNull(values);

        if (values.Length == 0)
        {
            throw new ArgumentException("Array must contain at least one value.", nameof(values));
        }

        var max = values[0];

        for (int i = 1; i < values.Length; i++)
        {
            if (values[i] > max)
            {
                max = values[i];
            }
        }

        return max;
    }

    /// <summary>
    /// Gets the lowest value from an array of dice values.
    /// </summary>
    /// <param name="values">Array of dice values.</param>
    /// <returns>Lowest value in the array.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="values"/> is empty.</exception>
    public static int Min(int[] values)
    {
        ArgumentNullException.ThrowIfNull(values);

        if (values.Length == 0)
        {
            throw new ArgumentException("Array must contain at least one value.", nameof(values));
        }

        var min = values[0];

        for (int i = 1; i < values.Length; i++)
        {
            if (values[i] < min)
            {
                min = values[i];
            }
        }

        return min;
    }
}
