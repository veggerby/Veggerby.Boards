using System;
using System.Collections.Generic;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.Risk.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Risk.Conditions;

/// <summary>
/// Validates that a FortifyGameEvent can be executed.
/// </summary>
/// <remarks>
/// Conditions:
/// - Must be in Fortify phase
/// - Player must own both territories
/// - Territories must be connected (path through owned territories)
/// - Source territory must retain at least 1 army after move
/// </remarks>
public sealed class FortifyCondition : IGameEventCondition<FortifyGameEvent>
{
    private readonly Game _game;

    /// <summary>
    /// Initializes a new instance of the <see cref="FortifyCondition"/> class.
    /// </summary>
    /// <param name="game">The game instance for path checking.</param>
    public FortifyCondition(Game game)
    {
        _game = game ?? throw new ArgumentNullException(nameof(game));
    }

    /// <inheritdoc />
    public ConditionResponse Evaluate(GameEngine engine, GameState state, FortifyGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(@event);

        // Check active player
        if (!state.TryGetActivePlayer(out var activePlayer))
        {
            return ConditionResponse.NotApplicable;
        }

        // Check phase
        var riskExtras = state.GetExtras<RiskStateExtras>();

        if (riskExtras is null)
        {
            return ConditionResponse.Fail("Risk state not found.");
        }

        if (riskExtras.CurrentPhase != RiskPhase.Fortify)
        {
            return ConditionResponse.Fail("Not in fortify phase.");
        }

        // Check territory ownership
        var fromState = state.GetState<TerritoryState>(@event.FromTerritory);
        var toState = state.GetState<TerritoryState>(@event.ToTerritory);

        if (fromState is null || toState is null)
        {
            return ConditionResponse.Fail("Territory state not found.");
        }

        if (!fromState.Owner.Equals(activePlayer) || !toState.Owner.Equals(activePlayer))
        {
            return ConditionResponse.Fail("Player must own both territories.");
        }

        // Check source has enough armies
        if (fromState.ArmyCount - @event.ArmyCount < 1)
        {
            return ConditionResponse.Fail("Must leave at least 1 army in the source territory.");
        }

        // Check connectivity via BFS
        if (!AreConnected(state, @event.FromTerritory, @event.ToTerritory, activePlayer))
        {
            return ConditionResponse.Fail("Territories are not connected through owned territories.");
        }

        return ConditionResponse.Valid;
    }

    /// <summary>
    /// Checks if two territories are connected through a chain of owned territories using BFS.
    /// </summary>
    private bool AreConnected(GameState state, Tile from, Tile to, Player owner)
    {
        if (from.Equals(to))
        {
            return true;
        }

        // Build owned territory set for quick lookup
        var ownedTiles = new HashSet<Tile>();

        foreach (var ts in state.GetStates<TerritoryState>())
        {
            if (ts.Owner.Equals(owner))
            {
                ownedTiles.Add(ts.Territory);
            }
        }

        // BFS from source to target
        var visited = new HashSet<Tile>();
        var queue = new Queue<Tile>();
        queue.Enqueue(from);
        visited.Add(from);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            foreach (var relation in _game.Board.TileRelations)
            {
                Tile? neighbor = null;

                if (relation.From.Equals(current))
                {
                    neighbor = relation.To;
                }
                else if (relation.To.Equals(current))
                {
                    neighbor = relation.From;
                }

                if (neighbor is not null && !visited.Contains(neighbor) && ownedTiles.Contains(neighbor))
                {
                    if (neighbor.Equals(to))
                    {
                        return true;
                    }

                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }
        }

        return false;
    }
}
