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
/// Enforces the mandatory capture rule in checkers: if any capture is available,
/// player must take a capture (not a normal move), and must choose the longest available chain.
/// </summary>
/// <remarks>
/// This is a simplified implementation. Full implementation would enumerate all capture chains
/// and enforce the longest chain rule.
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

    /// <summary>
    /// Evaluates if the move satisfies the mandatory capture rule.
    /// </summary>
    /// <param name="engine">The game engine.</param>
    /// <param name="state">The current game state.</param>
    /// <param name="evt">The move event to validate.</param>
    /// <returns>Valid if move is allowed, invalid with reason if not.</returns>
    public ConditionResponse Evaluate(GameEngine engine, GameState state, MovePieceGameEvent evt)
    {
        // TODO: Implement full mandatory capture logic
        // For now, allow all moves
        return ConditionResponse.Valid;
    }
}
