using System;
using System.Linq;
using Shouldly;
using Veggerby.Boards.Backgammon;
using Veggerby.Boards.Core;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Tests.Utils;
using Xunit;

namespace Veggerby.Boards.Tests.Backgammon
{
    public class BackgammonGameBuilderTests
    {
        private Tuple<string, string> Clockwise(string tileId)
        {
            return new Tuple<string, string>("clockwise", tileId);
        }

        private Tuple<string, string> CounterClockwise(string tileId)
        {
            return new Tuple<string, string>("counterclockwise", tileId);
        }

        [Fact]
        public void Should_initialize_game()
        {
            // arrange

            // act
            var actual = new BackgammonGameEngineBuilder().Compile();

            // assert

            // game
            actual.Game.ShouldHaveTileWithRelations("point-1", Clockwise("point-2"), CounterClockwise("home-black"));
            actual.Game.ShouldHaveTileWithRelations("point-2", Clockwise("point-3"), CounterClockwise("point-1"));
            actual.Game.ShouldHaveTileWithRelations("point-3", Clockwise("point-4"), CounterClockwise("point-2"));
            actual.Game.ShouldHaveTileWithRelations("point-4", Clockwise("point-5"), CounterClockwise("point-3"));
            actual.Game.ShouldHaveTileWithRelations("point-5", Clockwise("point-6"), CounterClockwise("point-4"));
            actual.Game.ShouldHaveTileWithRelations("point-6", Clockwise("point-7"), CounterClockwise("point-5"));
            actual.Game.ShouldHaveTileWithRelations("point-7", Clockwise("point-8"), CounterClockwise("point-6"));
            actual.Game.ShouldHaveTileWithRelations("point-8", Clockwise("point-9"), CounterClockwise("point-7"));
            actual.Game.ShouldHaveTileWithRelations("point-9", Clockwise("point-10"), CounterClockwise("point-8"));
            actual.Game.ShouldHaveTileWithRelations("point-10", Clockwise("point-11"), CounterClockwise("point-9"));
            actual.Game.ShouldHaveTileWithRelations("point-11", Clockwise("point-12"), CounterClockwise("point-10"));
            actual.Game.ShouldHaveTileWithRelations("point-12", Clockwise("point-13"), CounterClockwise("point-11"));
            actual.Game.ShouldHaveTileWithRelations("point-13", Clockwise("point-14"), CounterClockwise("point-12"));
            actual.Game.ShouldHaveTileWithRelations("point-14", Clockwise("point-15"), CounterClockwise("point-13"));
            actual.Game.ShouldHaveTileWithRelations("point-15", Clockwise("point-16"), CounterClockwise("point-14"));
            actual.Game.ShouldHaveTileWithRelations("point-16", Clockwise("point-17"), CounterClockwise("point-15"));
            actual.Game.ShouldHaveTileWithRelations("point-17", Clockwise("point-18"), CounterClockwise("point-16"));
            actual.Game.ShouldHaveTileWithRelations("point-18", Clockwise("point-19"), CounterClockwise("point-17"));
            actual.Game.ShouldHaveTileWithRelations("point-19", Clockwise("point-20"), CounterClockwise("point-18"));
            actual.Game.ShouldHaveTileWithRelations("point-20", Clockwise("point-21"), CounterClockwise("point-19"));
            actual.Game.ShouldHaveTileWithRelations("point-21", Clockwise("point-22"), CounterClockwise("point-20"));
            actual.Game.ShouldHaveTileWithRelations("point-22", Clockwise("point-23"), CounterClockwise("point-21"));
            actual.Game.ShouldHaveTileWithRelations("point-23", Clockwise("point-24"), CounterClockwise("point-22"));
            actual.Game.ShouldHaveTileWithRelations("point-24", Clockwise("home-white"), CounterClockwise("point-23"));

            actual.Game.ShouldHaveTileWithRelations("bar", Clockwise("point-1"), CounterClockwise("point-24"));

            actual.Game.GetArtifact<Dice>("dice-1").ShouldNotBeNull();
            actual.Game.GetArtifact<Dice>("dice-2").ShouldNotBeNull();
            actual.Game.GetArtifact<Dice>("doubling-dice").ShouldNotBeNull();

            // state
            actual.GameState.ShouldHavePieceState("white-1", "point-1");
            actual.GameState.ShouldHavePieceState("white-2", "point-1");

            actual.GameState.ShouldHavePieceState("white-3", "point-12");
            actual.GameState.ShouldHavePieceState("white-4", "point-12");
            actual.GameState.ShouldHavePieceState("white-5", "point-12");
            actual.GameState.ShouldHavePieceState("white-6", "point-12");
            actual.GameState.ShouldHavePieceState("white-7", "point-12");

            actual.GameState.ShouldHavePieceState("white-8", "point-17");
            actual.GameState.ShouldHavePieceState("white-9", "point-17");
            actual.GameState.ShouldHavePieceState("white-10", "point-17");

            actual.GameState.ShouldHavePieceState("white-11", "point-19");
            actual.GameState.ShouldHavePieceState("white-12", "point-19");
            actual.GameState.ShouldHavePieceState("white-13", "point-19");
            actual.GameState.ShouldHavePieceState("white-14", "point-19");
            actual.GameState.ShouldHavePieceState("white-15", "point-19");

            actual.GameState.ShouldHavePieceState("black-1", "point-24");
            actual.GameState.ShouldHavePieceState("black-2", "point-24");

            actual.GameState.ShouldHavePieceState("black-3", "point-13");
            actual.GameState.ShouldHavePieceState("black-4", "point-13");
            actual.GameState.ShouldHavePieceState("black-5", "point-13");
            actual.GameState.ShouldHavePieceState("black-6", "point-13");
            actual.GameState.ShouldHavePieceState("black-7", "point-13");

            actual.GameState.ShouldHavePieceState("black-8", "point-8");
            actual.GameState.ShouldHavePieceState("black-9", "point-8");
            actual.GameState.ShouldHavePieceState("black-10", "point-8");

            actual.GameState.ShouldHavePieceState("black-11", "point-6");
            actual.GameState.ShouldHavePieceState("black-12", "point-6");
            actual.GameState.ShouldHavePieceState("black-13", "point-6");
            actual.GameState.ShouldHavePieceState("black-14", "point-6");
            actual.GameState.ShouldHavePieceState("black-15", "point-6");
        }
    }
}