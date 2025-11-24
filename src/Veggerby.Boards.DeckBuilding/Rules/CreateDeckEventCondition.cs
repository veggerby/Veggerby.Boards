using Veggerby.Boards.Cards;
using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.States;

namespace Veggerby.Boards.DeckBuilding.Rules;

/// <summary>
/// Validates CreateDeckEvent payload for a deck: all declared piles must be present.
/// </summary>
public sealed class DeckBuildingCreateDeckEventCondition : IGameEventCondition<CreateDeckEvent>
{
    /// <inheritdoc />
    public ConditionResponse Evaluate(GameEngine engine, GameState state, CreateDeckEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(@event);
        foreach (var p in @event.Deck.Piles)
        {
            if (!@event.Piles.ContainsKey(p))
            {
                return ConditionResponse.Fail($"Missing pile '{p}'");
            }
        }
        return ConditionResponse.Valid;
    }
}