using System.Linq;
using Shouldly;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Artifacts.Patterns;
using Veggerby.Boards.Core.Phases;
using Veggerby.Boards.Core.States;
using Veggerby.Boards.Tests.Core.Fakes;
using Xunit;

namespace Veggerby.Boards.Tests.Core.Rules
{
    public class EndTurnRuleTests
    {
        public class Evaluate
        {
            [Fact]
            public void Should_return_next_turn_state()
            {
                // arrange
                var player1 = new Player("player-1");
                var player2 = new Player("player-2");
                var piece1 = new Piece("piece-1", player1, new [] { new AnyPattern() });
                var piece2 = new Piece("piece-2", player2, new [] { new AnyPattern() });
                var board = new TestBoard();
                var game = new Game("game", board, new [] { player1, player2 }, new[] { piece1, piece2 });
                var @event = new NullEvent();

                var rule = new SimpleEndTurnRule();
                var state = GameState.New(game, new[] { new TurnState(player1, new Turn(new Round(1), 1)) } );

                // act
                var actual = rule.Evaluate(game, state, @event);
                
                // assert
                actual.ChildStates.Count().ShouldBe(2);
                var turnState = actual.GetState<TurnState>(player2);
                turnState.Artifact.ShouldBe(player2);
                turnState.Round.Number.ShouldBe(1);                
                turnState.Turn.Number.ShouldBe(2);                
            }
            
            [Fact]
            public void Should_return_next_turn_next_round_state()
            {
                // arrange
                var player1 = new Player("player-1");
                var player2 = new Player("player-2");
                var piece1 = new Piece("piece-1", player1, new [] { new AnyPattern() });
                var piece2 = new Piece("piece-2", player2, new [] { new AnyPattern() });
                var board = new TestBoard();
                var game = new Game("game", board, new [] { player1, player2 }, new[] { piece1, piece2 });
                var @event = new NullEvent();

                var rule = new SimpleEndTurnRule();
                var state = GameState.New(game, new[] { new TurnState(player2, new Turn(new Round(1), 2)) } );

                // act
                var actual = rule.Evaluate(game, state, @event);
                
                // assert
                actual.ChildStates.Count().ShouldBe(2);
                var turnState = actual.GetState<TurnState>(player1);
                turnState.Artifact.ShouldBe(player1);
                turnState.Round.Number.ShouldBe(2);                
                turnState.Turn.Number.ShouldBe(1);      
            }
        }
    }
}