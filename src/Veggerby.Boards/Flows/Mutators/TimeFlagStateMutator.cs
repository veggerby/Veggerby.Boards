using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Events;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.States;
using Veggerby.Boards.Utilities.Scoring;

namespace Veggerby.Boards.Flows.Mutators;

/// <summary>
/// Mutator that ends the game when a player runs out of time.
/// </summary>
/// <remarks>
/// This mutator marks the game as ended and awards victory to the opponent.
/// It follows the standard termination pattern: adds both GameEndedState and
/// an outcome state describing the time forfeit.
/// Currently supports two-player games only. For multi-player games, consider
/// using a custom mutator that handles elimination differently.
/// </remarks>
public sealed class TimeFlagStateMutator : IStateMutator<TimeFlagEvent>
{
    private readonly Game _game;

    /// <summary>
    /// Initializes a new instance of the <see cref="TimeFlagStateMutator"/> class.
    /// </summary>
    /// <param name="game">The game instance.</param>
    public TimeFlagStateMutator(Game game)
    {
        ArgumentNullException.ThrowIfNull(game);

        _game = game;
    }

    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState gameState, TimeFlagEvent @event)
    {
        ArgumentNullException.ThrowIfNull(gameState);
        ArgumentNullException.ThrowIfNull(@event);

        if (gameState.GetStates<GameEndedState>().Any())
        {
            return gameState;
        }

        var allPlayers = _game.Artifacts.OfType<Player>().ToList();

        if (allPlayers.Count != 2)
        {
            throw new InvalidOperationException($"TimeFlagStateMutator currently supports only two-player games. Found {allPlayers.Count} players.");
        }

        var opponent = allPlayers.First(p => !p.Equals(@event.Player));

        var outcome = new OutcomeBuilder()
            .WithWinner(opponent, new() { ["Reason"] = "TimeExpired" })
            .WithLoser(@event.Player, new() { ["Reason"] = "TimeExpired" })
            .WithTerminalCondition("TimeExpired")
            .Build();

        return gameState.Next(new IArtifactState[]
        {
            new GameEndedState(),
            (StandardGameOutcome)outcome
        });
    }
}
