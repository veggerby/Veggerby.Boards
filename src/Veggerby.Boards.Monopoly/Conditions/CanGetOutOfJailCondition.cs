using System;
using System.Linq;

using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.Monopoly.Events;
using Veggerby.Boards.Monopoly.States;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Monopoly.Conditions;

/// <summary>
/// Condition that validates a player can get out of jail.
/// </summary>
public class CanGetOutOfJailCondition : IGameEventCondition<GetOutOfJailGameEvent>
{
    /// <inheritdoc />
    public ConditionResponse Evaluate(GameEngine engine, GameState state, GetOutOfJailGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(@event);

        var playerState = state.GetStates<MonopolyPlayerState>()
            .FirstOrDefault(ps => string.Equals(ps.Player.Id, @event.Player.Id, StringComparison.Ordinal));

        if (playerState is null)
        {
            return ConditionResponse.Ignore($"Player state not found for {@event.Player.Id}");
        }

        if (!playerState.InJail)
        {
            return ConditionResponse.Fail($"Player {@event.Player.Id} is not in jail");
        }

        switch (@event.Method)
        {
            case GetOutOfJailMethod.PaidFine:
                if (playerState.Cash < 50)
                {
                    return ConditionResponse.Fail($"Player {@event.Player.Id} has insufficient funds to pay $50 fine");
                }

                break;

            case GetOutOfJailMethod.UsedCard:
                if (!playerState.HasGetOutOfJailCard)
                {
                    return ConditionResponse.Fail($"Player {@event.Player.Id} does not have a Get Out of Jail Free card");
                }

                break;

            case GetOutOfJailMethod.RolledDoubles:
                // This is valid by default - doubles check happens elsewhere
                break;

            default:
                return ConditionResponse.Fail($"Unknown jail release method: {@event.Method}");
        }

        return ConditionResponse.Valid;
    }
}
