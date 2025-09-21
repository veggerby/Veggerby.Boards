using System;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.DecisionPlan;
using Veggerby.Boards.Flows.Phases;

namespace Veggerby.Boards;

/// <summary>
/// Encapsulates the immutable structural <see cref="Game"/> and the compiled root <see cref="GamePhase"/> tree.
/// </summary>
/// <remarks>
/// The engine itself contains no mutable state; progression happens through <c>GameProgress</c> which pairs
/// the engine with a current <c>GameState</c> and last processed event. The <see cref="GamePhaseRoot"/> is a
/// composite whose active leaf phase is resolved dynamically per state evaluation.
/// </remarks>
public class GameEngine
{
    /// <summary>
    /// Gets the immutable structural <see cref="Game"/> (board, players, artifacts).
    /// </summary>
    public Game Game { get; }

    /// <summary>
    /// Gets the root of the compiled phase tree used for resolving applicable rules.
    /// </summary>
    public GamePhase GamePhaseRoot { get; }

    /// <summary>
    /// Gets the optional precompiled decision plan (leaf phase ordering + rules). Null until
    /// feature flag <c>EnableDecisionPlan</c> is enabled at build time.
    /// </summary>
    internal DecisionPlan DecisionPlan { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="GameEngine"/> class.
    /// </summary>
    /// <param name="game">Immutable structural aggregate.</param>
    /// <param name="gamePhaseRoot">Root game phase (composite or leaf).</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="game"/> or <paramref name="gamePhaseRoot"/> is null.</exception>
    /// <param name="decisionPlan">Optional compiled decision plan (null when feature disabled).</param>
    public GameEngine(Game game, GamePhase gamePhaseRoot, DecisionPlan decisionPlan = null)
    {
        ArgumentNullException.ThrowIfNull(game);

        ArgumentNullException.ThrowIfNull(gamePhaseRoot);

        Game = game;
        GamePhaseRoot = gamePhaseRoot;
        DecisionPlan = decisionPlan; // may be null (feature flag disabled)
    }
}