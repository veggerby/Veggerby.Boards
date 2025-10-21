using System;
using System.Linq;

using Veggerby.Boards.Flows.Events;

namespace Veggerby.Boards.States;

/// <summary>
/// Legacy evaluation path kept only for DEBUG/test parity comparisons while the DecisionPlan pipeline hardens.
/// </summary>
/// <remarks>
/// Wrapped in conditional compilation so it is not shipped in release builds once parity is fully validated.
/// Determinism Note: This path must remain semantically identical to the pre-DecisionPlan traversal. Do NOT
/// modify logic except for mechanical refactors. Any divergence should be surfaced by parity tests.
/// </remarks>
#if DEBUG || TESTS
public partial class GameProgress
{
    [Obsolete("Legacy event traversal – only for debug parity. Will be removed once DecisionPlan is fully validated.")]
    private GameProgress HandleEventLegacy(IGameEvent @event)
    {
        var currentPhase = Engine.GamePhaseRoot.GetActiveGamePhase(State);
        if (currentPhase is null)
        {
            Engine.Observer.OnEventIgnored(@event, State);
            return this;
        }

        var events = currentPhase.PreProcessEvent(this, @event);
        return events.Aggregate(this, (seed, e) =>
        {
            var phaseForEvent = seed.Engine.GamePhaseRoot.GetActiveGamePhase(seed.State);
            if (phaseForEvent is null)
            {
                return seed; // nothing active; event ignored
            }

            seed.Engine.Observer.OnPhaseEnter(phaseForEvent, seed.State);
            var ruleCheckLocal = phaseForEvent.Rule.Check(seed.Engine, seed.State, e);
            seed.Engine.Observer.OnRuleEvaluated(phaseForEvent, phaseForEvent.Rule, ruleCheckLocal, seed.State, 0);

            if (ruleCheckLocal.Result == ConditionResult.Valid)
            {
                var newStateLocal = phaseForEvent.Rule.HandleEvent(seed.Engine, seed.State, e);
                seed.Engine.Observer.OnRuleApplied(phaseForEvent, phaseForEvent.Rule, e, seed.State, newStateLocal, 0);

                if (Internal.FeatureFlags.EnableStateHashing && newStateLocal.Hash.HasValue)
                {
                    seed.Engine.Observer.OnStateHashed(newStateLocal, newStateLocal.Hash.Value);
                }

                seed.Engine.Capabilities?.AccelerationContext?.OnStateTransition(seed.State, newStateLocal, e);
                return new GameProgress(seed.Engine, newStateLocal, seed.Events.Append(e));
            }
            else if (ruleCheckLocal.Result == ConditionResult.Invalid)
            {
                throw new InvalidGameEventException(e, ruleCheckLocal, seed.Game, phaseForEvent, seed.State);
            }
            else
            {
                seed.Engine.Observer.OnEventIgnored(e, seed.State);
                return seed;
            }
        });
    }
}
#else
public partial class GameProgress { }
#endif