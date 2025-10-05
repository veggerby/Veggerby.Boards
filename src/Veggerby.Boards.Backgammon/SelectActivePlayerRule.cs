using System.Linq;

using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Rules;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Backgammon;

/// <summary>
/// Rule that observes the opening dice roll and, when values differ, emits a <see cref="SelectActivePlayerGameEvent"/>.
/// </summary>
internal sealed class SelectActivePlayerRule(IGameEventRule inner) : IGameEventRule
{
    private readonly IGameEventRule _inner = inner;

    public ConditionResponse Check(GameEngine engine, GameState state, IGameEvent @event)
    {
        return _inner.Check(engine, state, @event);
    }

    public GameState HandleEvent(GameEngine engine, GameState state, IGameEvent @event)
    {
        var afterInner = _inner.HandleEvent(engine, state, @event);

        if (@event is RollDiceGameEvent<int> roll)
        {
            var diceStates = roll.NewDiceStates.ToList();
            if (diceStates.Count >= 2)
            {
                var d1 = diceStates[0];
                var d2 = diceStates[1];
                if (!d1.CurrentValue.Equals(d2.CurrentValue))
                {
                    var white = engine.Game.GetPlayer("white");
                    var black = engine.Game.GetPlayer("black");
                    var active = d1.CurrentValue > d2.CurrentValue ? white : black;
                    var selectEvent = new SelectActivePlayerGameEvent(active.Id);
                    var mutator = new SelectActivePlayerGameStateMutator();
                    return mutator.MutateState(engine, afterInner, selectEvent);
                }
            }
        }
        return afterInner;
    }
}