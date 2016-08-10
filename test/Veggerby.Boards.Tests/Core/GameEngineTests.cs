using System.Linq;
using Shouldly;
using Veggerby.Boards.Core;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Artifacts.Patterns;
using Veggerby.Boards.Core.Artifacts.Relations;
using Veggerby.Boards.Core.Events;
using Veggerby.Boards.Core.Rules;
using Veggerby.Boards.Core.States;
using Veggerby.Boards.Tests.Core.Fakes;
using Xunit;

namespace Veggerby.Boards.Tests.Core
{
    public class GameEngineTests
    {
        public class AddEvent
        {
            [Fact]
            public void Should_return_new_state()
            {
                // arrange
                var player = new Player("player");
                var board = new TestBoard();
                var piece = new Piece("id", player, new[] { new DirectionPattern(Direction.Clockwise) });
                var from = board.GetTile("tile-3");
                var to = board.GetTile("tile-6");

                var game = new Game(
                    "game",
                    board,
                    new [] { player },
                    new [] { piece });

                var rules = new RuleEngine(new[] { new PatternMovePieceRule()});

                var engine = GameEngine.New(
                    game, 
                    GameState.New(game, new [] { new PieceState(piece, from) }), 
                    rules);

                var @event = new MovePieceGameEvent(piece, from, to);

                // act
                var actual = engine.AddEvent(@event);
                
                // assert
                actual.ShouldBeTrue();
                engine.Events.Single().ShouldBe(@event);
                engine.GameState.ChildStates.Count().ShouldBe(2);
                engine.GameState.ChildStates.OfType<PieceState>().Count().ShouldBe(1);
                engine.GameState.ChildStates.OfType<TurnState>().Count().ShouldBe(1);
                var state = engine.GameState.GetState<PieceState>(piece);
                state.CurrentTile.ShouldBe(to);
            }

            [Fact]
            public void Should_not_return_new_state()
            {
                // arrange
                var player = new Player("player");
                var board = new TestBoard();
                var piece = new Piece("id", player, new[] { new DirectionPattern(Direction.CounterClockwise) });
                var from = board.GetTile("tile-3");
                var to = board.GetTile("tile-6");

                var game = new Game(
                    "game",
                    board,
                    new [] { player },
                    new [] { piece });

                var rules = new RuleEngine(new[] { new PatternMovePieceRule()});

                var engine = GameEngine.New(
                    game, 
                    GameState.New(game, new [] { new PieceState(piece, from)}), 
                    rules);

                var @event = new MovePieceGameEvent(piece, from, to);

                // act
                var actual = engine.AddEvent(@event);
                
                // assert
                actual.ShouldBeFalse();
                engine.Events.ShouldBeEmpty();
                engine.GameState.ChildStates.Count().ShouldBe(2);
                engine.GameState.ChildStates.OfType<PieceState>().Count().ShouldBe(1);
                engine.GameState.ChildStates.OfType<TurnState>().Count().ShouldBe(1);
                var state = engine.GameState.GetState<PieceState>(piece);
                state.CurrentTile.ShouldBe(from);
            }
        }
    }
}