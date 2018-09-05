using System.Linq;
using Shouldly;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.States;
using Veggerby.Boards.Tests.Core.Fakes;
using Xunit;

namespace Veggerby.Boards.Tests.Core
{
    public class GameEngineBuilderTests
    {
        public class Game_Builder_Tests
        {
            [Fact]
            public void Should_build_game()
            {
                // arrange
                var builder = new TestGameEngineBuilder();

                // act
                var actual = builder.Compile();

                // assert
                actual.ShouldNotBeNull();
                actual.Game.ShouldNotBeNull();
                actual.GameState.ShouldNotBeNull();
                actual.GamePhaseRoot.ShouldNotBeNull();

                actual.Game.Artifacts.Count().ShouldBe(7);
                actual.Game.Artifacts.Select(x => x.Id).ShouldBe(new [] { "piece-1", "piece-2", "piece-n", "piece-x", "piece-y", "dice", "dice-secondary" });

                actual.Game.Players.Count().ShouldBe(2);
                actual.Game.Players.Select(x => x.Id).ShouldBe(new [] { "player-1", "player-2" });

                actual.Game.Board.ShouldNotBeNull();
                actual.Game.Board.Id.ShouldBe("test");
                actual.Game.Board.Tiles.Count().ShouldBe(3);
                actual.Game.Board.Tiles.Select(x => x.Id).ShouldBe(new [] { "tile-1", "tile-2", "tile-3" });

                actual.Game.Board.TileRelations.Count().ShouldBe(3);
                actual.Game.Board.TileRelations.First().From.Id.ShouldBe("tile-1");
                actual.Game.Board.TileRelations.First().To.Id.ShouldBe("tile-2");
                actual.Game.Board.TileRelations.First().Direction.Id.ShouldBe("clockwise");

                actual.Game.Board.TileRelations.Skip(1).First().From.Id.ShouldBe("tile-2");
                actual.Game.Board.TileRelations.Skip(1).First().To.Id.ShouldBe("tile-1");
                actual.Game.Board.TileRelations.Skip(1).First().Direction.Id.ShouldBe("counterclockwise");

                actual.Game.Board.TileRelations.Last().From.Id.ShouldBe("tile-1");
                actual.Game.Board.TileRelations.Last().To.Id.ShouldBe("tile-3");
                actual.Game.Board.TileRelations.Last().Direction.Id.ShouldBe("up");
            }

            [Fact]
            public void Should_not_build_game_twice()
            {
                // arrange
                var builder = new TestGameEngineBuilder();
                var engine = builder.Compile();

                // act
                var actual = builder.Compile();

                // assert
                actual.ShouldBeSameAs(engine);
            }
        }

        public class Initial_GameState_Builder_Tests
        {
            [Fact]
            public void Should_initialize_game_state()
            {
                // arrange
                var builder = new TestGameEngineBuilder();

                // act
                var actual = builder.Compile();

                // assert
                actual.GameState.ShouldNotBeNull();
                actual.GameState.IsInitialState.ShouldBeTrue();
                actual.GameState.ChildStates.Count().ShouldBe(5);
                actual.GameState.ChildStates.OfType<PieceState>().Count().ShouldBe(3);
                actual.GameState.ChildStates.OfType<NullDiceState<int>>().Count().ShouldBe(1);
                actual.GameState.ChildStates.OfType<DiceState<int>>().Count().ShouldBe(1);

                var game = actual.Game;
                var piece1 = game.GetPiece("piece-1");
                var piece2 = game.GetPiece("piece-2");
                var pieceN = game.GetPiece("piece-n");

                var tile1 = game.GetTile("tile-1");
                var tile2 = game.GetTile("tile-2");

                var dice = game.GetArtifact<RegularDice>("dice");
                var dice2 = game.GetArtifact<RegularDice>("dice-secondary");


                var state1 = actual.GameState.GetState<PieceState>(piece1);
                var state2 = actual.GameState.GetState<PieceState>(piece2);
                var stateN = actual.GameState.GetState<PieceState>(pieceN);
                var stateDice2 = actual.GameState.GetState<DiceState<int>>(dice2);

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
}