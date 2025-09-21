using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Flows;

/// <summary>
/// Represents a single dice-driven movement step (with resulting state) in a multi-step path calculation.
/// </summary>
/// <remarks>
/// Steps are linked backwards through <see cref="Previous"/> enabling reconstruction of the full movement sequence via <see cref="Paths"/>.
/// </remarks>
public class SingleStepPath
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SingleStepPath"/> class.
    /// </summary>
    /// <param name="state">The resulting game state after applying this step.</param>
    /// <param name="diceState">The dice state consumed by the step.</param>
    /// <param name="path">The tile path traversed.</param>
    /// <param name="previous">Optional previous step in the sequence.</param>
    public SingleStepPath(GameState state, DiceState<int> diceState, TilePath path, SingleStepPath previous = null)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(diceState);
        ArgumentNullException.ThrowIfNull(path);

        NewState = state;
        DiceState = diceState;
        Path = path;
        Previous = previous;
    }

    /// <summary>
    /// Gets the game state after this movement step.
    /// </summary>
    public GameState NewState { get; }

    /// <summary>
    /// Gets the dice state consumed when performing this step.
    /// </summary>
    public DiceState<int> DiceState { get; }

    /// <summary>
    /// Gets the path segment for this step.
    /// </summary>
    public TilePath Path { get; }

    /// <summary>
    /// Gets the previous step (if any) in the path chain.
    /// </summary>
    public SingleStepPath Previous { get; }

    /// <summary>
    /// Gets the ordered sequence of path segments from the first step to this step.
    /// </summary>
    public IEnumerable<TilePath> Paths
    {
        get
        {
            if (Previous is null)
            {
                return [Path];
            }

            return [.. Previous.Paths, Path];
        }
    }
}