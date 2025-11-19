using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Builder.Extensions;
using Veggerby.Boards.Builder.Fluent;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.States;
using Veggerby.Boards.States.Conditions;

namespace Veggerby.Boards.Examples;

/// <summary>
/// Example game builder demonstrating the new fluent API.
/// </summary>
/// <remarks>
/// This example shows:
/// 1. Using AddRingTiles for circular board patterns
/// 2. Using DefineRules for lambda-scoped event handlers
/// 3. Using ConditionGroup for reusable condition patterns
/// 4. Using helper methods for better code organization
/// 
/// This is a simplified demonstration - not a complete playable game.
/// </remarks>
public class SimpleRaceGameBuilderExample : GameBuilder
{
    /// <summary>
    /// A simple race game where players move pieces around a circular track.
    /// </summary>
    protected override void Build()
    {
        BoardId = "simple-race-example";

        // Players
        AddPlayer("red");
        AddPlayer("blue");

        // Directions
        AddDirection("forward");
        AddDirection("backward");

        // Create a circular track using the fluent AddRingTiles method
        const int trackSize = 20;
        AddRingTiles(trackSize, i => $"track-{i}", (tile, position) =>
        {
            // Connect to next/previous tiles using helper functions
            var nextPos = NextInRing(position, trackSize);
            var prevPos = PreviousInRing(position, trackSize);

            tile
                .WithRelationTo($"track-{nextPos}")
                .InDirection("forward")
                .Done()
                .WithRelationTo($"track-{prevPos}")
                .InDirection("backward");
        });

        // Create pieces
        AddPiece("red-piece")
            .WithOwner("red")
            .HasDirection("forward").CanRepeat();

        AddPiece("blue-piece")
            .WithOwner("blue")
            .HasDirection("forward").CanRepeat();

        // Initial state
        WithPiece("red-piece").OnTile("track-0");
        WithPiece("blue-piece").OnTile("track-0");

        // Active player setup
        WithActivePlayer("red", true);
        WithActivePlayer("blue", false);

        // Game phases using new fluent DefineRules API
        // This demonstrates the improved readability with lambda scoping
        AddGamePhase("move-piece")
            .If<NullGameStateCondition>()
            .DefineRules(phase =>
            {
                // Move piece event with clear scoping
                phase.On<MovePieceGameEvent>(evt => evt
                    .When<PieceIsActivePlayerGameEventCondition>()
                    .And<PathNotObstructedGameEventCondition>()
                    .And<DestinationIsEmptyGameEventCondition>()
                    .Execute(m => m
                        .Apply<SimpleMovePieceMutator>()
                        .Apply(game => new NextPlayerStateMutator(new SingleActivePlayerGameStateCondition()))));
            })
            // Using the new WithEndGame helper for cleaner syntax
            .WithEndGame(
                game => new NullGameStateCondition(),
                game => new SimpleEndGameMutator());
    }
}

/// <summary>
/// Simple piece movement mutator for the example.
/// </summary>
internal class SimpleMovePieceMutator : IStateMutator<MovePieceGameEvent>
{
    public GameState MutateState(GameEngine engine, GameState state, MovePieceGameEvent @event)
    {
        // Simplified demonstration - actual implementation would move the piece
        // This just shows the pattern for a custom mutator
        return state;
    }
}

/// <summary>
/// Simple endgame mutator for the example.
/// </summary>
internal class SimpleEndGameMutator : IStateMutator<IGameEvent>
{
    public GameState MutateState(GameEngine engine, GameState state, IGameEvent @event)
    {
        return state.Next(new IArtifactState[] { new GameEndedState() });
    }
}

/// <summary>
/// Example condition group showing reusable patterns.
/// </summary>
internal static class ExampleRaceConditions
{
    /// <summary>
    /// Verifies the piece belongs to the active player.
    /// </summary>
    public static ConditionGroup<MovePieceGameEvent> ActivePlayerPiece =>
        ConditionGroup<MovePieceGameEvent>.Create()
            .Require<PieceIsActivePlayerGameEventCondition>();

    /// <summary>
    /// Verifies the path is clear.
    /// </summary>
    public static ConditionGroup<MovePieceGameEvent> ClearPath =>
        ConditionGroup<MovePieceGameEvent>.Create()
            .Require<PathNotObstructedGameEventCondition>()
            .Require<DestinationIsEmptyGameEventCondition>();
}
