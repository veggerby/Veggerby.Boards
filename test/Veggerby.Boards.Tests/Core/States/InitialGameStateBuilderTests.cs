using System.Linq;
using Shouldly;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.States;
using Veggerby.Boards.Tests.Core.Fakes;
using Xunit;

namespace Veggerby.Boards.Tests.Core.States
{
    public class InitialGameStateBuilderTests
    {
        [Fact]
        public void Should_initialize_game_state()
        {
            // arrange
            var game = new TestGameBuilder().Compile();
            var piece1 = game.GetPiece("piece-1");
            var piece2 = game.GetPiece("piece-2");
            var pieceN = game.GetPiece("piece-n");

            var tile1 = game.GetTile("tile-1");
            var tile2 = game.GetTile("tile-2");

            var dice = game.GetArtifact<RegularDice>("dice");
            var dice2 = game.GetArtifact<RegularDice>("dice-secondary");

            var builder = new TestInitialGameStateBuilder();

            // act
            var actual = builder.Compile(game);

            // assert
            actual.ShouldNotBeNull();
            actual.IsInitialState.ShouldBeTrue();
            actual.ChildStates.Count().ShouldBe(5);
            actual.ChildStates.OfType<PieceState>().Count().ShouldBe(3);
            actual.ChildStates.OfType<NullDiceState<int>>().Count().ShouldBe(1);
            actual.ChildStates.OfType<DiceState<int>>().Count().ShouldBe(1);

            var state1 = actual.GetState<PieceState>(piece1);
            var state2 = actual.GetState<PieceState>(piece2);
            var stateN = actual.GetState<PieceState>(pieceN);
            var stateDice2 = actual.GetState<DiceState<int>>(dice2);

            state1.ShouldNotBeNull();
            state1.CurrentTile.ShouldBe(tile1);

            state2.ShouldNotBeNull();
            state2.CurrentTile.ShouldBe(tile2);

            stateN.ShouldNotBeNull();
            stateN.CurrentTile.ShouldBe(tile1);

            stateDice2.ShouldNotBeNull();
            stateDice2.CurrentValue.ShouldBe(4);
        }
    }
}