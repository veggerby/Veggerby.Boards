using System;
using System.Collections.Generic;

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
    /// Gets the evaluation observer used for instrumentation (never null).
    /// </summary>
    internal IEvaluationObserver Observer { get; }

    private readonly Veggerby.Boards.Internal.Tracing.EvaluationTrace _lastTrace;
    private readonly EngineServices _services;
    /// <summary>
    /// Gets the internal engine services container (compiled patterns, future bitboards). Internal for extension wiring.
    /// </summary>
    internal EngineServices Services => _services;

    /// <summary>
    /// Gets the last evaluation trace (if trace capture feature enabled); otherwise <c>null</c>.
    /// </summary>
    internal Veggerby.Boards.Internal.Tracing.EvaluationTrace LastTrace => _lastTrace;

    /// <summary>
    /// Initializes a new instance of the <see cref="GameEngine"/> class.
    /// </summary>
    /// <param name="game">Immutable structural aggregate.</param>
    /// <param name="gamePhaseRoot">Root game phase (composite or leaf).</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="game"/> or <paramref name="gamePhaseRoot"/> is null.</exception>
    /// <param name="decisionPlan">Optional compiled decision plan (null when feature disabled).</param>
    /// <param name="observer">Evaluation observer (null replaced with <see cref="NullEvaluationObserver"/>).</param>
    /// <param name="services">Optional internal service container (compiled patterns, etc.).</param>
    public GameEngine(Game game, GamePhase gamePhaseRoot, DecisionPlan decisionPlan = null, IEvaluationObserver observer = null, EngineServices services = null)
    {
        ArgumentNullException.ThrowIfNull(game);

        ArgumentNullException.ThrowIfNull(gamePhaseRoot);

        Game = game;
        GamePhaseRoot = gamePhaseRoot;
        DecisionPlan = decisionPlan; // may be null (feature flag disabled)
        _services = services ?? EngineServices.Empty;
        var baseObserver = observer ?? NullEvaluationObserver.Instance;
        if (Internal.FeatureFlags.EnableTraceCapture)
        {
            _lastTrace = new Internal.Tracing.EvaluationTrace();
            Observer = new Internal.Tracing.TraceCaptureObserver(baseObserver, _lastTrace);
        }
        else
        {
            Observer = baseObserver;
            _lastTrace = null;
        }
    }
}