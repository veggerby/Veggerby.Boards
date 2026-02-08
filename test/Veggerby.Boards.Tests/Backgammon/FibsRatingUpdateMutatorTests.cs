using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Backgammon;
using Veggerby.Boards.Backgammon.Mutators;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Tests.Backgammon;

public class FibsRatingUpdateMutatorTests
{
    [Fact]
    public void Should_update_ratings_after_game_ends()
    {
        // arrange
        var builder = new BackgammonGameBuilder();
        var progress = builder.Compile().WithFibsRatings(matchLength: 1);
        var game = progress.Game;
        var engine = progress.Engine;

        var white = game.GetPlayer("white")!;
        var black = game.GetPlayer("black")!;

        var outcomeState = new BackgammonOutcomeState(white, BackgammonVictoryType.Normal);
        var endedState = new GameEndedState();

        var state = progress.State.Next(new IArtifactState[] { endedState, outcomeState });

        var mutator = new FibsRatingUpdateMutator(game);
        var dummyEvent = new DummyEvent();

        // act
        var newState = mutator.MutateState(engine, state, dummyEvent);

        // assert
        var whiteRating = newState.GetStates<FibsRatingState>()
            .FirstOrDefault(r => Equals(r.Player, white));
        var blackRating = newState.GetStates<FibsRatingState>()
            .FirstOrDefault(r => Equals(r.Player, black));

        whiteRating.Should().NotBeNull();
        blackRating.Should().NotBeNull();

        whiteRating!.Rating.Should().BeGreaterThan(1500.0);
        whiteRating.Experience.Should().Be(1);

        blackRating!.Rating.Should().BeLessThan(1500.0);
        blackRating.Experience.Should().Be(1);
    }

    [Fact]
    public void Should_not_update_ratings_for_unlimited_match()
    {
        // arrange
        var builder = new BackgammonGameBuilder();
        var progress = builder.Compile().WithFibsRatings(matchLength: 0, isUnlimited: true);
        var game = progress.Game;
        var engine = progress.Engine;

        var white = game.GetPlayer("white")!;

        var outcomeState = new BackgammonOutcomeState(white, BackgammonVictoryType.Normal);
        var endedState = new GameEndedState();

        var state = progress.State.Next(new IArtifactState[] { endedState, outcomeState });

        var mutator = new FibsRatingUpdateMutator(game);
        var dummyEvent = new DummyEvent();

        // act
        var newState = mutator.MutateState(engine, state, dummyEvent);

        // assert
        var whiteRating = newState.GetStates<FibsRatingState>()
            .FirstOrDefault(r => Equals(r.Player, white));

        whiteRating!.Rating.Should().Be(1500.0);
        whiteRating.Experience.Should().Be(0);
    }

    [Fact]
    public void Should_not_update_ratings_when_no_match_config()
    {
        // arrange
        var builder = new BackgammonGameBuilder();
        var progress = builder.Compile();
        var game = progress.Game;
        var engine = progress.Engine;

        var white = game.GetPlayer("white")!;

        var outcomeState = new BackgammonOutcomeState(white, BackgammonVictoryType.Normal);
        var endedState = new GameEndedState();

        var state = progress.State.Next(new IArtifactState[] { endedState, outcomeState });

        var mutator = new FibsRatingUpdateMutator(game);
        var dummyEvent = new DummyEvent();

        // act
        var newState = mutator.MutateState(engine, state, dummyEvent);

        // assert
        newState.Should().Be(state);
    }

    [Fact]
    public void Should_not_update_ratings_when_no_outcome()
    {
        // arrange
        var builder = new BackgammonGameBuilder();
        var progress = builder.Compile().WithFibsRatings(matchLength: 1);
        var game = progress.Game;
        var engine = progress.Engine;

        var mutator = new FibsRatingUpdateMutator(game);
        var dummyEvent = new DummyEvent();

        // act
        var newState = mutator.MutateState(engine, progress.State, dummyEvent);

        // assert
        newState.Should().Be(progress.State);
    }

    [Fact]
    public void Should_apply_correct_experience_and_rating_changes()
    {
        // arrange
        var builder = new BackgammonGameBuilder();
        var progress = builder.Compile().WithFibsRatings(
            matchLength: 5,
            whiteRating: 1600.0,
            whiteExperience: 50,
            blackRating: 1500.0,
            blackExperience: 75);

        var game = progress.Game;
        var engine = progress.Engine;

        var black = game.GetPlayer("black")!;

        var outcomeState = new BackgammonOutcomeState(black, BackgammonVictoryType.Gammon);
        var endedState = new GameEndedState();

        var state = progress.State.Next(new IArtifactState[] { endedState, outcomeState });

        var mutator = new FibsRatingUpdateMutator(game);
        var dummyEvent = new DummyEvent();

        // act
        var newState = mutator.MutateState(engine, state, dummyEvent);

        // assert
        var blackRating = newState.GetStates<FibsRatingState>()
            .FirstOrDefault(r => Equals(r.Player, black));

        blackRating.Should().NotBeNull();
        blackRating!.Rating.Should().BeGreaterThan(1500.0);
        blackRating.Experience.Should().Be(80);
    }

    private sealed class DummyEvent : IGameEvent
    {
    }
}
