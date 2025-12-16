using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Checkers.Conditions;

/// <summary>
/// Placeholder for mandatory capture rule enforcement in checkers.
/// Currently allows all moves; full enforcement pending implementation.
/// </summary>
/// <remarks>
/// <para>
/// When complete, this condition will enforce the mandatory capture rule: if any capture is available,
/// player must take a capture (not a normal move), and must choose the longest available chain.
/// </para>
/// <para>
/// See Known Limitations in README.md for details on mandatory capture implementation status.
/// </para>
/// </remarks>
public sealed class MandatoryCaptureCondition : IGameEventCondition<MovePieceGameEvent>
{
    private readonly Game _game;

    /// <summary>
    /// Initializes a new instance of the <see cref="MandatoryCaptureCondition"/> class.
    /// </summary>
    /// <param name="game">The game instance.</param>
    public MandatoryCaptureCondition(Game game)
    {
        _game = game;
    }

    /// <inheritdoc />
    public ConditionResponse Evaluate(GameEngine engine, GameState state, MovePieceGameEvent evt)
    {
        // Known Limitation: Full mandatory capture logic not yet implemented.
        // See README.md Known Limitations section.
        // Future implementation will:
        // 1. Enumerate all possible moves for active player's pieces
        // 2. Identify which moves are captures (jumps)
        // 3. If captures exist, validate that the current move is a capture
        // 4. Enforce longest-chain rule among available captures

        return ConditionResponse.Valid;
    }
}
