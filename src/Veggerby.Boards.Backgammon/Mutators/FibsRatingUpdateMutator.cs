using System;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Backgammon.Mutators;

/// <summary>
/// Mutator that updates FIBS ratings for players when a match is completed.
/// </summary>
/// <remarks>
/// This mutator should be invoked after game completion (when BackgammonOutcomeState is added).
/// It reads the match configuration and current player ratings, calculates rating changes using
/// the FIBS formula, and updates the rating states with new values.
/// </remarks>
public sealed class FibsRatingUpdateMutator : IStateMutator<IGameEvent>
{
    private readonly Game _game;

    /// <summary>
    /// Initializes a new instance of the <see cref="FibsRatingUpdateMutator"/> class.
    /// </summary>
    /// <param name="game">The backgammon game definition.</param>
    public FibsRatingUpdateMutator(Game game)
    {
        _game = game ?? throw new ArgumentNullException(nameof(game));
    }

    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState state, IGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(@event);

        var matchConfig = state.GetStates<FibsMatchConfigState>().FirstOrDefault();

        if (matchConfig is null)
        {
            return state;
        }

        if (matchConfig.IsUnlimited)
        {
            return state;
        }

        var outcome = state.GetStates<BackgammonOutcomeState>().FirstOrDefault();

        if (outcome is null)
        {
            return state;
        }

        var winner = outcome.Winner;
        var players = _game.Players.ToList();
        var loser = players.FirstOrDefault(p => !Equals(p, winner));

        if (loser is null)
        {
            return state;
        }

        var winnerRatingState = state.GetStates<FibsRatingState>().FirstOrDefault(r => Equals(r.Player, winner));
        var loserRatingState = state.GetStates<FibsRatingState>().FirstOrDefault(r => Equals(r.Player, loser));

        if (winnerRatingState is null || loserRatingState is null)
        {
            return state;
        }

        var (winnerResult, loserResult) = FibsRatingCalculator.CalculateRatingChanges(
            winnerRatingState.Rating,
            winnerRatingState.Experience,
            loserRatingState.Rating,
            loserRatingState.Experience,
            matchConfig.MatchLength);

        var newWinnerRatingState = new FibsRatingState(winner, winnerResult.NewRating, winnerResult.NewExperience);
        var newLoserRatingState = new FibsRatingState(loser, loserResult.NewRating, loserResult.NewExperience);

        return state.Next(new IArtifactState[] { newWinnerRatingState, newLoserRatingState });
    }
}
