using System.Linq;
using Shouldly;
using Veggerby.Boards.Tests.Core.Fakes;
using Xunit;

namespace Veggerby.Boards.Tests.Core.Artifacts
{
    public class GameBuilderTests
    {
        [Fact]
        public void Should_build_game()
        {
            // arrange
            var builder = new TestGameBuilder();

            // act
            var actual = builder.Compile();

            // assert
            actual.ShouldNotBeNull();

            actual.Artifacts.Count().ShouldBe(7);
            actual.Artifacts.Select(x => x.Id).ShouldBe(new [] { "piece-1", "piece-2", "piece-n", "piece-x", "piece-y", "dice", "dice-secondary" });

            actual.Players.Count().ShouldBe(2);
            actual.Players.Select(x => x.Id).ShouldBe(new [] { "player-1", "player-2" });

            actual.Board.ShouldNotBeNull();
            actual.Board.Id.ShouldBe("test");
            actual.Board.Tiles.Count().ShouldBe(2);
            actual.Board.Tiles.Select(x => x.Id).ShouldBe(new [] { "tile-1", "tile-2" });

            actual.Board.TileRelations.Count().ShouldBe(2);
            actual.Board.TileRelations.First().From.Id.ShouldBe("tile-1");
            actual.Board.TileRelations.First().To.Id.ShouldBe("tile-2");
            actual.Board.TileRelations.First().Direction.Id.ShouldBe("clockwise");

            actual.Board.TileRelations.Last().From.Id.ShouldBe("tile-2");
            actual.Board.TileRelations.Last().To.Id.ShouldBe("tile-1");
            actual.Board.TileRelations.Last().Direction.Id.ShouldBe("counterclockwise");
        }

        [Fact]
        public void Should_not_build_game_twice()
        {
            // arrange
            var builder = new TestGameBuilder();
            var game = builder.Compile();

            // act
            var actual = builder.Compile();

            // assert
            actual.ShouldBeSameAs(game);
        }
    }
}