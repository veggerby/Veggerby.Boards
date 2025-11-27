using System.Linq;

using Veggerby.Boards.Monopoly;
using Veggerby.Boards.Monopoly.Events;
using Veggerby.Boards.Monopoly.States;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Tests.Monopoly;

public class MonopolyGameBuilderTests
{
    [Fact]
    public void Compile_WithDefaultSettings_Creates40Squares()
    {
        // arrange
        var builder = new MonopolyGameBuilder();

        // act
        var progress = builder.Compile();

        // assert
        var tiles = progress.Engine.Game.Board.Tiles.ToList();
        tiles.Count.Should().Be(40);
    }

    [Fact]
    public void Compile_WithDefaultSettings_Creates4Players()
    {
        // arrange
        var builder = new MonopolyGameBuilder();

        // act
        var progress = builder.Compile();

        // assert
        var players = progress.Engine.Game.Players.ToList();
        players.Count.Should().Be(4);
    }

    [Fact]
    public void Compile_With2Players_Creates2Players()
    {
        // arrange
        var builder = new MonopolyGameBuilder(playerCount: 2);

        // act
        var progress = builder.Compile();

        // assert
        var players = progress.Engine.Game.Players.ToList();
        players.Count.Should().Be(2);
    }

    [Fact]
    public void Compile_AllPiecesStartOnGo()
    {
        // arrange
        var builder = new MonopolyGameBuilder(playerCount: 2);

        // act
        var progress = builder.Compile();

        // assert
        var pieceStates = progress.State.GetStates<PieceState>().ToList();
        pieceStates.Count.Should().Be(2);
        foreach (var ps in pieceStates)
        {
            ps.CurrentTile?.Id.Should().Be("square-0"); // Go is at position 0
        }
    }

    [Fact]
    public void Compile_BoardIsCircular()
    {
        // arrange
        var builder = new MonopolyGameBuilder();

        // act
        var progress = builder.Compile();

        // assert
        // Verify we can traverse from Go back to Go using TileRelations
        var game = progress.Engine.Game;
        var currentTile = game.Board.GetTile("square-0");
        currentTile.Should().NotBeNull();

        for (int i = 0; i < 40; i++)
        {
            var relations = game.Board.TileRelations.Where(r => r.From.Equals(currentTile)).ToList();
            relations.Should().NotBeEmpty($"Tile {currentTile!.Id} should have at least one relation");
            currentTile = relations[0].To;
        }

        currentTile!.Id.Should().Be("square-0", "After 40 steps, we should be back at Go");
    }

}
