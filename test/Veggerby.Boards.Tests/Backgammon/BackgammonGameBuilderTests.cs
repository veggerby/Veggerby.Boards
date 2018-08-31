using System;
using System.Linq;
using Shouldly;
using Veggerby.Boards.Backgammon;
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
            var actual = new BackgammonGameBuilder().Compile();

            // assert
            actual.ShouldHaveTileWithRelations("point-1", Clockwise("point-2"), CounterClockwise("home-black"));
            actual.ShouldHaveTileWithRelations("point-2", Clockwise("point-3"), CounterClockwise("point-1"));
            actual.ShouldHaveTileWithRelations("point-3", Clockwise("point-4"), CounterClockwise("point-2"));
            actual.ShouldHaveTileWithRelations("point-4", Clockwise("point-5"), CounterClockwise("point-3"));
            actual.ShouldHaveTileWithRelations("point-5", Clockwise("point-6"), CounterClockwise("point-4"));
            actual.ShouldHaveTileWithRelations("point-6", Clockwise("point-7"), CounterClockwise("point-5"));
            actual.ShouldHaveTileWithRelations("point-7", Clockwise("point-8"), CounterClockwise("point-6"));
            actual.ShouldHaveTileWithRelations("point-8", Clockwise("point-9"), CounterClockwise("point-7"));
            actual.ShouldHaveTileWithRelations("point-9", Clockwise("point-10"), CounterClockwise("point-8"));
            actual.ShouldHaveTileWithRelations("point-10", Clockwise("point-11"), CounterClockwise("point-9"));
            actual.ShouldHaveTileWithRelations("point-11", Clockwise("point-12"), CounterClockwise("point-10"));
            actual.ShouldHaveTileWithRelations("point-12", Clockwise("point-13"), CounterClockwise("point-11"));
            actual.ShouldHaveTileWithRelations("point-13", Clockwise("point-14"), CounterClockwise("point-12"));
            actual.ShouldHaveTileWithRelations("point-14", Clockwise("point-15"), CounterClockwise("point-13"));
            actual.ShouldHaveTileWithRelations("point-15", Clockwise("point-16"), CounterClockwise("point-14"));
            actual.ShouldHaveTileWithRelations("point-16", Clockwise("point-17"), CounterClockwise("point-15"));
            actual.ShouldHaveTileWithRelations("point-17", Clockwise("point-18"), CounterClockwise("point-16"));
            actual.ShouldHaveTileWithRelations("point-18", Clockwise("point-19"), CounterClockwise("point-17"));
            actual.ShouldHaveTileWithRelations("point-19", Clockwise("point-20"), CounterClockwise("point-18"));
            actual.ShouldHaveTileWithRelations("point-20", Clockwise("point-21"), CounterClockwise("point-19"));
            actual.ShouldHaveTileWithRelations("point-21", Clockwise("point-22"), CounterClockwise("point-20"));
            actual.ShouldHaveTileWithRelations("point-22", Clockwise("point-23"), CounterClockwise("point-21"));
            actual.ShouldHaveTileWithRelations("point-23", Clockwise("point-24"), CounterClockwise("point-22"));
            actual.ShouldHaveTileWithRelations("point-24", Clockwise("home-white"), CounterClockwise("point-23"));

            actual.ShouldHaveTileWithRelations("bar", Clockwise("point-1"), CounterClockwise("point-24"));

            actual.GetArtifact<Dice<int>>("dice-1").ShouldNotBeNull();
            actual.GetArtifact<Dice<int>>("dice-2").ShouldNotBeNull();
            actual.GetArtifact<Dice<int>>("doubling-dice").ShouldNotBeNull();
        }
    }
}