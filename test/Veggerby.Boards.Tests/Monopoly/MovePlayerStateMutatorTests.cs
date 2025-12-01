using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Monopoly;
using Veggerby.Boards.Monopoly.Events;
using Veggerby.Boards.Monopoly.Mutators;
using Veggerby.Boards.Monopoly.States;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Tests.Monopoly;

public class MovePlayerStateMutatorTests
{
    private static (GameProgress Progress, Player Player) SetupGame()
    {
        var builder = new MonopolyGameBuilder(playerCount: 2, playerNames: ["Alice", "Bob"]);
        var progress = builder.Compile();

        // Initialize player states
        var initialPlayerStates = builder.CreateInitialPlayerStates(progress).ToList();
        progress = progress.NewState(initialPlayerStates);

        var player = progress.Engine.Game.GetPlayer("Alice")!;

        return (progress, player);
    }

    [Fact]
    public void MovePlayer_FromGo_UpdatesPiecePosition()
    {
        // arrange
        var (progress, player) = SetupGame();
        var mutator = new MovePlayerStateMutator();
        var gameEvent = new MovePlayerGameEvent(player, 3, 4); // 7 spaces

        // act
        var newState = mutator.MutateState(progress.Engine, progress.State, gameEvent);

        // assert
        var piece = progress.Engine.Game.Artifacts.OfType<Piece>()
            .First(p => p.Owner?.Id == player.Id);
        var pieceState = newState.GetState<PieceState>(piece);
        pieceState!.CurrentTile!.Id.Should().Be("square-7");
    }

    [Fact]
    public void MovePlayer_WithDoubles_IncrementsConsecutiveDoubles()
    {
        // arrange
        var (progress, player) = SetupGame();
        var mutator = new MovePlayerStateMutator();
        var gameEvent = new MovePlayerGameEvent(player, 3, 3); // Doubles

        // act
        var newState = mutator.MutateState(progress.Engine, progress.State, gameEvent);

        // assert
        var playerState = newState.GetStates<MonopolyPlayerState>()
            .First(ps => ps.Player.Equals(player));
        playerState.ConsecutiveDoubles.Should().Be(1);
    }

    [Fact]
    public void MovePlayer_WithoutDoubles_ResetsConsecutiveDoubles()
    {
        // arrange
        var (progress, player) = SetupGame();

        // Set up player with 2 consecutive doubles
        var initialPlayerState = progress.State.GetStates<MonopolyPlayerState>()
            .First(ps => ps.Player.Equals(player));
        var updatedPlayerState = initialPlayerState.WithConsecutiveDoubles(2);
        progress = progress.NewState([updatedPlayerState]);

        var mutator = new MovePlayerStateMutator();
        var gameEvent = new MovePlayerGameEvent(player, 3, 4); // Not doubles

        // act
        var newState = mutator.MutateState(progress.Engine, progress.State, gameEvent);

        // assert
        var playerState = newState.GetStates<MonopolyPlayerState>()
            .First(ps => ps.Player.Equals(player));
        playerState.ConsecutiveDoubles.Should().Be(0);
    }

    [Fact]
    public void MovePlayer_PassingGo_Adds200DollarsToCash()
    {
        // arrange
        var (progress, player) = SetupGame();

        // Move player to position 35 (close to Go)
        var piece = progress.Engine.Game.Artifacts.OfType<Piece>()
            .First(p => p.Owner?.Id == player.Id);
        var tile35 = progress.Engine.Game.Board.GetTile("square-35")!;
        progress = progress.NewState([new PieceState(piece, tile35)]);

        var mutator = new MovePlayerStateMutator();
        var gameEvent = new MovePlayerGameEvent(player, 4, 3); // 7 spaces, wrapping to square-2

        var initialCash = progress.State.GetStates<MonopolyPlayerState>()
            .First(ps => ps.Player.Equals(player)).Cash;

        // act
        var newState = mutator.MutateState(progress.Engine, progress.State, gameEvent);

        // assert
        var playerState = newState.GetStates<MonopolyPlayerState>()
            .First(ps => ps.Player.Equals(player));
        playerState.Cash.Should().Be(initialCash + 200);
    }

    [Fact]
    public void MovePlayer_NotPassingGo_DoesNotAddCash()
    {
        // arrange
        var (progress, player) = SetupGame();

        // Move player to position 5
        var piece = progress.Engine.Game.Artifacts.OfType<Piece>()
            .First(p => p.Owner?.Id == player.Id);
        var tile5 = progress.Engine.Game.Board.GetTile("square-5")!;
        progress = progress.NewState([new PieceState(piece, tile5)]);

        var mutator = new MovePlayerStateMutator();
        var gameEvent = new MovePlayerGameEvent(player, 3, 4); // 7 spaces, to square-12

        var initialCash = progress.State.GetStates<MonopolyPlayerState>()
            .First(ps => ps.Player.Equals(player)).Cash;

        // act
        var newState = mutator.MutateState(progress.Engine, progress.State, gameEvent);

        // assert
        var playerState = newState.GetStates<MonopolyPlayerState>()
            .First(ps => ps.Player.Equals(player));
        playerState.Cash.Should().Be(initialCash); // No change
    }

    [Fact]
    public void MovePlayer_LandingExactlyOnGo_CollectsPassGoBonus()
    {
        // arrange
        var (progress, player) = SetupGame();

        // Move player to position 33
        var piece = progress.Engine.Game.Artifacts.OfType<Piece>()
            .First(p => p.Owner?.Id == player.Id);
        var tile33 = progress.Engine.Game.Board.GetTile("square-33")!;
        progress = progress.NewState([new PieceState(piece, tile33)]);

        var mutator = new MovePlayerStateMutator();
        var gameEvent = new MovePlayerGameEvent(player, 4, 3); // 7 spaces, lands exactly on square-0 (Go)

        var initialCash = progress.State.GetStates<MonopolyPlayerState>()
            .First(ps => ps.Player.Equals(player)).Cash;

        // act
        var newState = mutator.MutateState(progress.Engine, progress.State, gameEvent);

        // assert
        var pieceState = newState.GetState<PieceState>(piece);
        pieceState!.CurrentTile!.Id.Should().Be("square-0");

        // In Monopoly, both passing and landing on Go award the $200 bonus.
        // The current logic treats landing on Go as passing Go (newPosition < currentPosition && currentPosition != 0),
        // so the bonus is awarded in both cases. This test verifies the landing case.
        var playerState = newState.GetStates<MonopolyPlayerState>()
            .First(ps => ps.Player.Equals(player));
        playerState.Cash.Should().Be(initialCash + 200);
    }
}
