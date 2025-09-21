using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Artifacts.Patterns;
using Veggerby.Boards.Core.Artifacts.Relations;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.Flows.Rules.Conditions;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Flows;

/// <summary>
/// Explores reachable multi-step movement paths for a piece using available dice values and movement patterns.
/// </summary>
/// <remarks>
/// Path discovery is breadth-like: it expands all currently reachable partial paths until at least one reaches the destination
/// or no new expansions are possible. Dice are consumed (replaced by <see cref="NullDiceState"/>) when a step uses them.
/// </remarks>
public class SingleStepPathFinder
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SingleStepPathFinder"/> class.
    /// </summary>
    /// <param name="stepMoveCondition">Condition that must validate each single step movement event.</param>
    /// <param name="dice">The dice whose values drive movement distances.</param>
    public SingleStepPathFinder(IGameEventCondition<MovePieceGameEvent> stepMoveCondition, params Dice[] dice)
    {
        ArgumentNullException.ThrowIfNull(stepMoveCondition);
        ArgumentNullException.ThrowIfNull(dice);

        if (!dice.Any())
        {
            throw new ArgumentException(nameof(dice));
        }

        StepMoveCondition = stepMoveCondition;
        Dice = dice;
    }

    /// <summary>
    /// Gets the condition evaluated for each single movement step.
    /// </summary>
    public IGameEventCondition<MovePieceGameEvent> StepMoveCondition { get; }

    /// <summary>
    /// Gets the dice referenced for movement distance resolution.
    /// </summary>
    public Dice[] Dice { get; }

    private SingleStepPath GetSingleStep(GameEngine engine, PieceState pieceState, IPattern pattern, GameState state, DiceState<int> diceState, Tile to, SingleStepPath previousStep = null)
    {
        var visitor = new ResolveTilePathDistanceVisitor(engine.Game.Board, pieceState.CurrentTile, to, diceState.CurrentValue, false);
        pattern.Accept(visitor);

        if (visitor.ResultPath is null)
        {
            return null;
        }

        var step = StepMoveCondition.Evaluate(engine, state, new MovePieceGameEvent(pieceState.Artifact, visitor.ResultPath));

        if (step.Result != ConditionResult.Valid)
        {
            return null;
        }

        var newState = state.Next([
            new PieceState(pieceState.Artifact, visitor.ResultPath.To),
            new NullDiceState(diceState.Artifact)
        ]);

        return new SingleStepPath(newState, diceState, visitor.ResultPath, previousStep);
    }

    private IEnumerable<SingleStepPath> FindSingleSteps(GameEngine engine, Piece piece, GameState state, Tile to, SingleStepPath previousStep)
    {
        var pieceState = state.GetState<PieceState>(piece);

        if (pieceState is null)
        {
            return Enumerable.Empty<SingleStepPath>();
        }

        var diceStates = Dice.Select(x => state.GetState<DiceState<int>>(x)).Where(x => x is not null);

        if (!diceStates.Any())
        {
            return Enumerable.Empty<SingleStepPath>();
        }

        return [.. piece
            .Patterns
            .SelectMany(pattern => diceStates.Select(diceState => GetSingleStep(engine, pieceState, pattern, state, diceState, to, previousStep)))
            .Where(x => x is not null)];
    }

    private IEnumerable<SingleStepPath> ContinueStep(GameEngine engine, Piece piece, Tile to, SingleStepPath step)
    {
        return FindSingleSteps(engine, piece, step.NewState, to, step);
    }

    /// <summary>
    /// Discovers all reachable paths (sequences of steps) that move a piece from a source tile to a target tile using available dice.
    /// </summary>
    /// <param name="engine">The game engine.</param>
    /// <param name="state">The starting state.</param>
    /// <param name="piece">The piece to move.</param>
    /// <param name="from">The starting tile (must match current piece location).</param>
    /// <param name="to">The destination tile.</param>
    /// <returns>All successful paths ending on <paramref name="to"/>.</returns>
    public IEnumerable<SingleStepPath> GetPaths(GameEngine engine, GameState state, Piece piece, Tile from, Tile to)
    {
        var pieceState = state.GetState<PieceState>(piece);

        if (!pieceState.CurrentTile.Equals(from))
        {
            return Enumerable.Empty<SingleStepPath>();
        }

        var steps = FindSingleSteps(engine, piece, state, to, null);
        while (steps.Any() && !steps.Any(x => x.Path.To.Equals(to)))
        {
            steps = [.. steps.SelectMany(step => ContinueStep(engine, piece, to, step))];
        }

        return [.. steps.Where(x => x.Path.To.Equals(to))];
    }
}