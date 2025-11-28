using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.Risk.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Risk.Mutators;

/// <summary>
/// Transitions from Attack phase to Fortify phase.
/// </summary>
public sealed class EndAttackPhaseStateMutator : IStateMutator<EndAttackPhaseGameEvent>
{
    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState gameState, EndAttackPhaseGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(gameState);
        ArgumentNullException.ThrowIfNull(@event);

        var riskExtras = gameState.GetExtras<RiskStateExtras>();

        if (riskExtras is null)
        {
            throw new InvalidOperationException("Risk state not found.");
        }

        var newRiskExtras = riskExtras with { CurrentPhase = RiskPhase.Fortify };

        return gameState.ReplaceExtras(newRiskExtras);
    }
}

/// <summary>
/// Ends the turn after Fortify phase and starts next player's turn.
/// </summary>
public sealed class EndFortifyPhaseStateMutator : IStateMutator<EndFortifyPhaseGameEvent>
{
    private readonly IReadOnlyList<Continent> _continents;

    /// <summary>
    /// Initializes a new instance of the <see cref="EndFortifyPhaseStateMutator"/> class.
    /// </summary>
    /// <param name="continents">Continent definitions for reinforcement calculation.</param>
    public EndFortifyPhaseStateMutator(IReadOnlyList<Continent> continents)
    {
        _continents = continents ?? throw new ArgumentNullException(nameof(continents));
    }

    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState gameState, EndFortifyPhaseGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(gameState);
        ArgumentNullException.ThrowIfNull(@event);

        // Rotate to next player
        var players = engine.Game.Players.ToList();
        var currentPlayer = gameState.GetActivePlayer();
        var currentIndex = players.IndexOf(currentPlayer);
        var nextIndex = (currentIndex + 1) % players.Count;
        var nextPlayer = players[nextIndex];

        // Calculate reinforcements for next player
        var reinforcements = ReinforcementCalculator.Calculate(nextPlayer, gameState, _continents);

        // Update extras
        var riskExtras = gameState.GetExtras<RiskStateExtras>();

        if (riskExtras is null)
        {
            throw new InvalidOperationException("Risk state not found.");
        }

        var newRiskExtras = riskExtras with
        {
            CurrentPhase = RiskPhase.Reinforce,
            ReinforcementsRemaining = reinforcements,
            ConqueredThisTurn = false
        };

        var updatedState = gameState.ReplaceExtras(newRiskExtras);

        // Update active player projections
        var newStates = new List<IArtifactState>();

        foreach (var player in players)
        {
            newStates.Add(new ActivePlayerState(player, player.Equals(nextPlayer)));
        }

        return updatedState.Next(newStates);
    }
}

/// <summary>
/// Skips Fortify phase and ends turn (same as EndFortifyPhase but without fortification).
/// </summary>
public sealed class SkipFortifyPhaseStateMutator : IStateMutator<SkipFortifyPhaseGameEvent>
{
    private readonly IReadOnlyList<Continent> _continents;

    /// <summary>
    /// Initializes a new instance of the <see cref="SkipFortifyPhaseStateMutator"/> class.
    /// </summary>
    /// <param name="continents">Continent definitions for reinforcement calculation.</param>
    public SkipFortifyPhaseStateMutator(IReadOnlyList<Continent> continents)
    {
        _continents = continents ?? throw new ArgumentNullException(nameof(continents));
    }

    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState gameState, SkipFortifyPhaseGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(gameState);
        ArgumentNullException.ThrowIfNull(@event);

        // Same logic as EndFortifyPhaseStateMutator
        var players = engine.Game.Players.ToList();
        var currentPlayer = gameState.GetActivePlayer();
        var currentIndex = players.IndexOf(currentPlayer);
        var nextIndex = (currentIndex + 1) % players.Count;
        var nextPlayer = players[nextIndex];

        var reinforcements = ReinforcementCalculator.Calculate(nextPlayer, gameState, _continents);

        var riskExtras = gameState.GetExtras<RiskStateExtras>();

        if (riskExtras is null)
        {
            throw new InvalidOperationException("Risk state not found.");
        }

        var newRiskExtras = riskExtras with
        {
            CurrentPhase = RiskPhase.Reinforce,
            ReinforcementsRemaining = reinforcements,
            ConqueredThisTurn = false
        };

        var updatedState = gameState.ReplaceExtras(newRiskExtras);

        var newStates = new List<IArtifactState>();

        foreach (var player in players)
        {
            newStates.Add(new ActivePlayerState(player, player.Equals(nextPlayer)));
        }

        return updatedState.Next(newStates);
    }
}
