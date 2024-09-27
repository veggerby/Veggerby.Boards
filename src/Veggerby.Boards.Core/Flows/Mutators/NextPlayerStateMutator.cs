using System;
using System.Linq;

using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Flows.Mutators;

public class NextPlayerStateMutator : IStateMutator<IGameEvent>
{
    public NextPlayerStateMutator(IGameStateCondition condition)
    {
        ArgumentNullException.ThrowIfNull(condition);

        Condition = condition;
    }

    public IGameStateCondition Condition { get; }

    public GameState MutateState(GameEngine engine, GameState gameState, IGameEvent @event)
    {
        var response = Condition.Evaluate(gameState);

        if (response.Result != ConditionResult.Valid)
        {
            return gameState;
        }

        var activePlayerStates = gameState.GetStates<ActivePlayerState>();
        var activePlayerState = activePlayerStates.Single(x => x.IsActive);

        var nextPlayer = engine
            .Game
            .Players
            .Concat(engine.Game.Players) // to allow "loop around|
            .SkipWhile(x => !x.Equals(activePlayerState.Artifact)) // find active player
            .Skip(1) // skip active player
            .First(); // take next

        var previousPlayerState = new ActivePlayerState(activePlayerState.Artifact, false);
        var nextPlayerState = new ActivePlayerState(nextPlayer, true);

        return gameState.Next([previousPlayerState, nextPlayerState]);
    }
}