using Veggerby.Boards.DeckBuilding.Artifacts;
using Veggerby.Boards.DeckBuilding.Events;
using Veggerby.Boards.DeckBuilding.States;
using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.States;
namespace Veggerby.Boards.DeckBuilding.Rules;

/// <summary>
/// Allows registering a card definition if one does not already exist for the card id.
/// </summary>
public sealed class RegisterCardDefinitionEventCondition : IGameEventCondition<RegisterCardDefinitionEvent>
{
    /// <inheritdoc />
    public ConditionResponse Evaluate(GameEngine engine, GameState state, RegisterCardDefinitionEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(@event);
        foreach (var existing in state.GetStates<CardDefinitionState>())
        {
            if (existing.Artifact.Id == @event.CardId)
            {
                return ConditionResponse.Fail("Definition already registered");
            }
        }
        return ConditionResponse.Valid;
    }
}