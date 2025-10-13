using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.States;

namespace Veggerby.Boards.DeckBuilding;

/// <summary>
/// Creates a <see cref="CardDefinitionState"/> for a registered definition artifact.
/// </summary>
public sealed class RegisterCardDefinitionStateMutator : IStateMutator<RegisterCardDefinitionEvent>
{
    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState state, RegisterCardDefinitionEvent @event)
    {
        // Build definition artifact (id stable with card id) and wrap in state.
        // @event.Types is IReadOnlyList<string>; CardDefinition expects IList<string> so wrap in List.
        var definition = new CardDefinition(@event.CardId, @event.Name, new System.Collections.Generic.List<string>(@event.Types), @event.Cost, @event.VictoryPoints);
        var definitionState = new CardDefinitionState(definition);
        return state.Next([definitionState]);
    }
}