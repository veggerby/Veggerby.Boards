using System.Collections.Generic;
using System.Linq;


using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Rules;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Flows.Phases;

/// <summary>
/// A phase that delegates activity to one of its child phases based on their individual conditions.
/// </summary>
public class CompositeGamePhase : GamePhase
{
    private readonly IList<GamePhase> _childPhases;

    /// <summary>
    /// Gets an immutable snapshot of configured child phases.
    /// </summary>
    public IEnumerable<GamePhase> ChildPhases => _childPhases.ToList().AsReadOnly();

        internal CompositeGamePhase(int number, string label, IGameStateCondition condition, CompositeGamePhase? parent, IEnumerable<IGameEventPreProcessor> preProcessors)
            : base(number, label, condition, GameEventRule<IGameEvent>.Null, parent, preProcessors)
    {
        _childPhases = [];
    }

    /// <inheritdoc />
    public override GamePhase? GetActiveGamePhase(GameState gameState)
    {
        if (!Condition.Evaluate(gameState).Equals(ConditionResponse.Valid))
        {
            return null;
        }

        return _childPhases
            .Select(x => x.GetActiveGamePhase(gameState))
            .FirstOrDefault(x => x is not null);
    }

    internal void Add(GamePhase phase)
    {
        _childPhases.Add(phase);
    }
}