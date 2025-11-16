using System;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.DecisionPlan;
using Veggerby.Boards.Flows.Observers;
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
    public Game Game
    {
        get;
    }

    /// <summary>
    /// Gets the root of the compiled phase tree used for resolving applicable rules.
    /// </summary>
    public GamePhase GamePhaseRoot
    {
        get;
    }

    /// <summary>
    /// Gets the compiled decision plan (leaf phase ordering + rules). Always non-null (legacy traversal removed).
    /// </summary>
    internal DecisionPlan DecisionPlan
    {
        get;
    }

    /// <summary>
    /// Gets the evaluation observer used for instrumentation (never null).
    /// </summary>
    internal IEvaluationObserver Observer
    {
        get;
    }

    /// <summary>
    /// Gets the optional capability set (may be null when no experimental subsystems enabled).
    /// </summary>
    internal EngineCapabilities? Capabilities
    {
        get;
    }

    /// <summary>
    /// Gets the last evaluation trace (if trace capture feature enabled); otherwise <c>null</c>.
    /// </summary>
    internal Internal.Tracing.EvaluationTrace? LastTrace
    {
        get;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GameEngine"/> class.
    /// </summary>
    /// <param name="game">Immutable structural aggregate.</param>
    /// <param name="gamePhaseRoot">Root game phase (composite or leaf).</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="game"/> or <paramref name="gamePhaseRoot"/> is null.</exception>
    /// <param name="decisionPlan">Optional compiled decision plan (null when feature disabled).</param>
    /// <param name="observer">Evaluation observer (null replaced with <see cref="NullEvaluationObserver"/>).</param>
    public GameEngine(Game game, GamePhase gamePhaseRoot, DecisionPlan decisionPlan, IEvaluationObserver? observer = null)
        : this(game, gamePhaseRoot, decisionPlan, observer, null)
    {
    }

    /// <summary>
    /// Internal constructor allowing capability injection (builder / tests).
    /// </summary>
    /// <param name="game">Immutable structural aggregate.</param>
    /// <param name="gamePhaseRoot">Root game phase (composite or leaf).</param>
    /// <param name="decisionPlan">Optional compiled decision plan (null when feature disabled).</param>
    /// <param name="observer">Evaluation observer (null replaced with <see cref="NullEvaluationObserver"/>).</param>
    /// <param name="capabilities">Optional capability set (compiled patterns, bitboards, occupancy, etc.).</param>
    internal GameEngine(Game game, GamePhase gamePhaseRoot, DecisionPlan decisionPlan, IEvaluationObserver? observer, EngineCapabilities? capabilities)
    {
        ArgumentNullException.ThrowIfNull(game);

        ArgumentNullException.ThrowIfNull(gamePhaseRoot);

        Game = game;
        GamePhaseRoot = gamePhaseRoot;
        ArgumentNullException.ThrowIfNull(decisionPlan);
        DecisionPlan = decisionPlan;
        Capabilities = capabilities; // may be null (no experimental features)
        var baseObserver = observer ?? NullEvaluationObserver.Instance;
        // Observer batching removed (experimental feature deferred to future performance story)

        // Always enable trace capture (graduated feature) - available when observer configured
        LastTrace = new Internal.Tracing.EvaluationTrace();
        Observer = new Internal.Tracing.TraceCaptureObserver(baseObserver, LastTrace);
    }
}