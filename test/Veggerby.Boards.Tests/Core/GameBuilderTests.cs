using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.States;
using Veggerby.Boards.Tests.Core.Fakes;

namespace Veggerby.Boards.Tests.Core;

public class GameBuilderTests
{
    public class Game_Builder_Tests
    {
        [Fact]
        public void Should_build_game()
        {
            // arrange
            var builder = new TestGameBuilder();

            // act
            var actual = builder.Compile();

            // assert
            actual.Should().NotBeNull();
            actual.Game.Should().NotBeNull();
            actual.State.Should().NotBeNull();
            actual.Engine.Should().NotBeNull();

            var expectedArtifacts = Boards.Internal.FeatureFlags.EnableTurnSequencing
                ? new[] { "piece-1", "piece-2", "piece-n", "piece-x", "piece-y", "dice", "dice-secondary", "artifact-x", "turn-timeline" }
                : new[] { "piece-1", "piece-2", "piece-n", "piece-x", "piece-y", "dice", "dice-secondary", "artifact-x" };
            actual.Game.Artifacts.Count().Should().Be(expectedArtifacts.Length);
            actual.Game.Artifacts.Select(x => x.Id).Should().Equal(expectedArtifacts);

            actual.Game.Players.Count().Should().Be(2);
            actual.Game.Players.Select(x => x.Id).Should().Equal(["player-1", "player-2"]);

            actual.Game.Board.Should().NotBeNull();
            actual.Game.Board.Id.Should().Be("test");
            actual.Game.Board.Tiles.Count().Should().Be(3);
            actual.Game.Board.Tiles.Select(x => x.Id).Should().Equal(["tile-1", "tile-2", "tile-3"]);

            actual.Game.Board.TileRelations.Count().Should().Be(3);
            actual.Game.Board.TileRelations.First().From.Id.Should().Be("tile-1");
            actual.Game.Board.TileRelations.First().To.Id.Should().Be("tile-2");
            actual.Game.Board.TileRelations.First().Direction.Id.Should().Be("clockwise");

            actual.Game.Board.TileRelations.Skip(1).First().From.Id.Should().Be("tile-2");
            actual.Game.Board.TileRelations.Skip(1).First().To.Id.Should().Be("tile-1");
            actual.Game.Board.TileRelations.Skip(1).First().Direction.Id.Should().Be("counterclockwise");

            actual.Game.Board.TileRelations.Last().From.Id.Should().Be("tile-1");
            actual.Game.Board.TileRelations.Last().To.Id.Should().Be("tile-3");
            actual.Game.Board.TileRelations.Last().Direction.Id.Should().Be("up");
        }

        [Fact]
        public void Should_not_build_game_twice()
        {
            // arrange
            var builder = new TestGameBuilder();
            var engine = builder.Compile();

            // act
            var actual = builder.Compile();

            // assert
            actual.Should().Be(engine);
        }
    }

    public class Initial_GameState_Builder_Tests
    {
        [Fact]
        public void Should_initialize_game_state()
        {
            // arrange
            var builder = new TestGameBuilder();

            // act
            var actual = builder.Compile();

            // assert
            actual.State.Should().NotBeNull();
            actual.State.IsInitialState.Should().BeTrue();
            var expectedStateCount = Boards.Internal.FeatureFlags.EnableTurnSequencing ? 6 : 5;
            actual.State.ChildStates.Count().Should().Be(expectedStateCount);
            actual.State.ChildStates.OfType<PieceState>().Count().Should().Be(3);
            actual.State.ChildStates.OfType<NullDiceState>().Count().Should().Be(1);
            actual.State.ChildStates.OfType<DiceState<int>>().Count().Should().Be(1);

            var game = actual.Game;
            var piece1 = game.GetPiece("piece-1");
            var piece2 = game.GetPiece("piece-2");
            var pieceN = game.GetPiece("piece-n");

            var tile1 = game.GetTile("tile-1");
            var tile2 = game.GetTile("tile-2");

            var dice = game.GetArtifact<Dice>("dice");
            var dice2 = game.GetArtifact<Dice>("dice-secondary");


            var state1 = actual.State.GetState<PieceState>(piece1);
            var state2 = actual.State.GetState<PieceState>(piece2);
            var stateN = actual.State.GetState<PieceState>(pieceN);
            var stateDice2 = actual.State.GetState<DiceState<int>>(dice2);

            state1.Should().NotBeNull();
            state1.CurrentTile.Should().Be(tile1);

            state2.Should().NotBeNull();
            state2.CurrentTile.Should().Be(tile2);

            stateN.Should().NotBeNull();
            stateN.CurrentTile.Should().Be(tile1);

            stateDice2.Should().NotBeNull();
            stateDice2.CurrentValue.Should().Be(4);
        }
    }
}