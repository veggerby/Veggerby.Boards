using System.Linq;
using Shouldly;
using Veggerby.Boards.Backgammon;
using Veggerby.Boards.Tests.Utils;
using Xunit;

namespace Veggerby.Boards.Tests.Backgammon
{
    public class BackgammonInitialGameStateBuilderTests
    {
        [Fact]
        public void Should_initialize_game_state()
        {
            // arrange
            var backgammon = new BackgammonGameBuilder().Compile();

            // act
            var actual = new BackgammonInitialGameStateBuilder().Compile(backgammon);

            // assert
            actual.ShouldHavePieceState("white-1", "point-1");
            actual.ShouldHavePieceState("white-2", "point-1");

            actual.ShouldHavePieceState("white-3", "point-12");
            actual.ShouldHavePieceState("white-4", "point-12");
            actual.ShouldHavePieceState("white-5", "point-12");
            actual.ShouldHavePieceState("white-6", "point-12");
            actual.ShouldHavePieceState("white-7", "point-12");

            actual.ShouldHavePieceState("white-8", "point-17");
            actual.ShouldHavePieceState("white-9", "point-17");
            actual.ShouldHavePieceState("white-10", "point-17");

            actual.ShouldHavePieceState("white-11", "point-19");
            actual.ShouldHavePieceState("white-12", "point-19");
            actual.ShouldHavePieceState("white-13", "point-19");
            actual.ShouldHavePieceState("white-14", "point-19");
            actual.ShouldHavePieceState("white-15", "point-19");

            actual.ShouldHavePieceState("black-1", "point-24");
            actual.ShouldHavePieceState("black-2", "point-24");

            actual.ShouldHavePieceState("black-3", "point-13");
            actual.ShouldHavePieceState("black-4", "point-13");
            actual.ShouldHavePieceState("black-5", "point-13");
            actual.ShouldHavePieceState("black-6", "point-13");
            actual.ShouldHavePieceState("black-7", "point-13");

            actual.ShouldHavePieceState("black-8", "point-8");
            actual.ShouldHavePieceState("black-9", "point-8");
            actual.ShouldHavePieceState("black-10", "point-8");

            actual.ShouldHavePieceState("black-11", "point-6");
            actual.ShouldHavePieceState("black-12", "point-6");
            actual.ShouldHavePieceState("black-13", "point-6");
            actual.ShouldHavePieceState("black-14", "point-6");
            actual.ShouldHavePieceState("black-15", "point-6");
       }
    }
}