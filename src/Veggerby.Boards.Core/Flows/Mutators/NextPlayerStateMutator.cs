using System;
using System.Linq;

using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Flows.Mutators;

/// <summary>
/// Mutator that advances the active player to the next player in the engine's player sequence when a supplied condition is valid.
/// </summary>
/// <remarks>
/// The player sequence is treated as circular. The condition allows deferring turn advancement until specific state criteria are met
/// (e.g., all dice consumed, no pending mandatory moves, etc.). If the condition does not evaluate to <see cref="ConditionResult.Valid"/>,
/// the game state is returned unchanged.
/// </remarks>
public class NextPlayerStateMutator : IStateMutator<IGameEvent>
{
    /// <summary>
    /// Initializes a new instance of the mutator.
    /// </summary>
    /// <param name="condition">Condition controlling when the turn should advance.</param>
    public NextPlayerStateMutator(IGameStateCondition condition)
    {
        ArgumentNullException.ThrowIfNull(condition);

        Condition = condition;
    }

    /// <summary>
    /// Gets the condition that must be valid for the active player to advance.
    /// </summary>
    public IGameStateCondition Condition { get; }

    /// <inheritdoc />
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