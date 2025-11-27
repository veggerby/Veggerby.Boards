using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Risk.Mutators;

/// <summary>
/// Checks for world domination win condition and ends the game if achieved.
/// </summary>
public sealed class RiskEndGameMutator : IStateMutator<IGameEvent>
{
    private readonly Game _game;

    /// <summary>
    /// Initializes a new instance of the <see cref="RiskEndGameMutator"/> class.
    /// </summary>
    /// <param name="game">The game instance.</param>
    public RiskEndGameMutator(Game game)
    {
        _game = game ?? throw new ArgumentNullException(nameof(game));
    }

    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState gameState, IGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(gameState);
        ArgumentNullException.ThrowIfNull(@event);

        // Check if already ended
        if (gameState.GetStates<GameEndedState>().Any())
        {
            return gameState;
        }

        // Check for world domination
        var territoryStates = gameState.GetStates<TerritoryState>().ToList();

        if (territoryStates.Count == 0)
        {
            return gameState;
        }

        var firstOwner = territoryStates[0].Owner;
        var allSameOwner = true;

        for (int i = 1; i < territoryStates.Count; i++)
        {
            if (!territoryStates[i].Owner.Equals(firstOwner))
            {
                allSameOwner = false;
                break;
            }
        }

        if (allSameOwner)
        {
            // Game over - world domination achieved
            var eliminationOrder = new List<Artifacts.Player>();

            // TODO: Track elimination order during game (for now, empty)
            var outcomeState = new RiskOutcomeState(firstOwner, eliminationOrder);

            return gameState.Next(new IArtifactState[] { new GameEndedState(), outcomeState });
        }

        return gameState;
    }
}
