using System.Linq;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Artifacts.Patterns;
using Veggerby.Boards.Core.Artifacts.Relations;
using Veggerby.Boards.Core.Rules;
using Veggerby.Boards.Core.States;
using Veggerby.Boards.Tests.Core.Fakes;
using Xunit;

namespace Veggerby.Boards.Tests.Core.Rules
{
    public class CompositeRuleTests
    {
        public class Check
        {
            [Fact]
            public void Should_return_simple_result()
            {
                // arrange
                var player = new Player("player");
                var board = new TestBoard();
                var piece = new Piece("id", player, new[] { new DirectionPattern(Direction.CounterClockwise) });
                var game = new Game(
                    "game",
                    board,
                    new [] { player },
                    new [] { piece });

                var innerRule1 = new SimpleRule(RuleCheckState.Valid);

                var rule = new CompositeRule(new[] { innerRule1 }); 

                // act
                var actual = rule.Check(game, GameState.New(game, null), new NullEvent());
                
                // assert
                Assert.Equal(RuleCheckState.Valid, actual);
                Assert.Equal(1, innerRule1.CheckCallCount);
                Assert.Equal(1, innerRule1.EvaluateCallCount); // to properly chain... evaluate is called after successfull check
            }

            [Fact]
            public void Should_return_simple_result_multiple()
            {
                // arrange
                var player = new Player("player");
                var board = new TestBoard();
                var piece = new Piece("id", player, new[] { new DirectionPattern(Direction.CounterClockwise) });
                var game = new Game(
                    "game",
                    board,
                    new [] { player },
                    new [] { piece });

                var innerRule1 = new SimpleRule(RuleCheckState.Valid);
                var innerRule2 = new SimpleRule(RuleCheckState.Valid);

                var rule = new CompositeRule(new[] { innerRule1, innerRule2 }); 

                // act
                var actual = rule.Check(game, GameState.New(game, null), new NullEvent());
                
                // assert
                Assert.Equal(RuleCheckState.Valid, actual);
                Assert.Equal(1, innerRule1.CheckCallCount);
                Assert.Equal(1, innerRule1.EvaluateCallCount); // to properly chain... evaluate is called after successfull check
                Assert.Equal(1, innerRule2.CheckCallCount);
                Assert.Equal(1, innerRule2.EvaluateCallCount); // to properly chain... evaluate is called after successfull check
            }

            [Fact]
            public void Should_return_simple_result_multiple_one_invalid()
            {
                // arrange
                var player = new Player("player");
                var board = new TestBoard();
                var piece = new Piece("id", player, new[] { new DirectionPattern(Direction.CounterClockwise) });
                var game = new Game(
                    "game",
                    board,
                    new [] { player },
                    new [] { piece });

                var innerRule1 = new SimpleRule(RuleCheckState.Valid);
                var innerRule2 = new SimpleRule(RuleCheckState.Invalid);

                var rule = new CompositeRule(new[] { innerRule1, innerRule2 }); 

                // act
                var actual = rule.Check(game, GameState.New(game, null), new NullEvent());
                
                // assert
                Assert.Equal(RuleCheckState.Invalid, actual); // allRules = true by default
                Assert.Equal(1, innerRule1.CheckCallCount);
                Assert.Equal(1, innerRule1.EvaluateCallCount); // to properly chain... evaluate is called after successfull check
                Assert.Equal(1, innerRule2.CheckCallCount);
                Assert.Equal(0, innerRule2.EvaluateCallCount); // to properly chain... evaluate is called after successfull check
            }

            [Fact]
            public void Should_return_simple_result_multiple_one_not_applicable()
            {
                // arrange
                var player = new Player("player");
                var board = new TestBoard();
                var piece = new Piece("id", player, new[] { new DirectionPattern(Direction.CounterClockwise) });
                var game = new Game(
                    "game",
                    board,
                    new [] { player },
                    new [] { piece });

                var innerRule1 = new SimpleRule(RuleCheckState.NotApplicable);
                var innerRule2 = new SimpleRule(RuleCheckState.Valid);
                var innerRule3 = new SimpleRule(RuleCheckState.Valid);

                var rule = new CompositeRule(new[] { innerRule1, innerRule2, innerRule3 }); 

                // act
                var actual = rule.Check(game, GameState.New(game, null), new NullEvent());
                
                // assert
                Assert.Equal(RuleCheckState.Valid, actual); // allRules = true by default
                Assert.Equal(1, innerRule1.CheckCallCount);
                Assert.Equal(0, innerRule1.EvaluateCallCount); // to properly chain... evaluate is called after successfull check
                Assert.Equal(1, innerRule2.CheckCallCount);
                Assert.Equal(1, innerRule2.EvaluateCallCount); // to properly chain... evaluate is called after successfull check
                Assert.Equal(1, innerRule3.CheckCallCount);
                Assert.Equal(1, innerRule3.EvaluateCallCount); // to properly chain... evaluate is called after successfull check
            }

            [Fact]
            public void Should_return_simple_result_multiple_one_not_applicable_any()
            {
                // arrange
                var player = new Player("player");
                var board = new TestBoard();
                var piece = new Piece("id", player, new[] { new DirectionPattern(Direction.CounterClockwise) });
                var game = new Game(
                    "game",
                    board,
                    new [] { player },
                    new [] { piece });

                var innerRule1 = new SimpleRule(RuleCheckState.NotApplicable);
                var innerRule2 = new SimpleRule(RuleCheckState.Valid);
                var innerRule3 = new SimpleRule(RuleCheckState.Valid);

                var rule = new CompositeRule(new[] { innerRule1, innerRule2, innerRule3 }, false); 

                // act
                var actual = rule.Check(game, GameState.New(game, null), new NullEvent());
                
                // assert
                Assert.Equal(RuleCheckState.Valid, actual); // allRules = true by default
                Assert.Equal(1, innerRule1.CheckCallCount);
                Assert.Equal(0, innerRule1.EvaluateCallCount); // to properly chain... evaluate is called after successfull check
                Assert.Equal(1, innerRule2.CheckCallCount);
                Assert.Equal(1, innerRule2.EvaluateCallCount); // to properly chain... evaluate is called after successfull check
                Assert.Equal(0, innerRule3.CheckCallCount); // not called due to breaking out
                Assert.Equal(0, innerRule3.EvaluateCallCount); // to properly chain... evaluate is called after successfull check
            }

            [Fact]
            public void Should_return_simple_result_multiple_two_invalid_any()
            {
                // arrange
                var player = new Player("player");
                var board = new TestBoard();
                var piece = new Piece("id", player, new[] { new DirectionPattern(Direction.CounterClockwise) });
                var game = new Game(
                    "game",
                    board,
                    new [] { player },
                    new [] { piece });

                var innerRule1 = new SimpleRule(RuleCheckState.Invalid);
                var innerRule2 = new SimpleRule(RuleCheckState.Valid);
                var innerRule3 = new SimpleRule(RuleCheckState.Invalid);

                var rule = new CompositeRule(new[] { innerRule1, innerRule2, innerRule3 }, false); 

                // act
                var actual = rule.Check(game, GameState.New(game, null), new NullEvent());
                
                // assert
                Assert.Equal(RuleCheckState.Valid, actual); // allRules = true by default
                Assert.Equal(1, innerRule1.CheckCallCount);
                Assert.Equal(0, innerRule1.EvaluateCallCount); // to properly chain... evaluate is called after successfull check
                Assert.Equal(1, innerRule2.CheckCallCount);
                Assert.Equal(1, innerRule2.EvaluateCallCount); // to properly chain... evaluate is called after successfull check
                Assert.Equal(0, innerRule3.CheckCallCount); // not called due to breaking out
                Assert.Equal(0, innerRule3.EvaluateCallCount); // to properly chain... evaluate is called after successfull check
            }
        }

        public class Evaluate
        {
            [Fact]
            public void Should_return_simple_result()
            {
                // arrange
                var player = new Player("player");
                var board = new TestBoard();
                var piece = new Piece("id", player, new[] { new DirectionPattern(Direction.CounterClockwise) });
                var game = new Game(
                    "game",
                    board,
                    new [] { player },
                    new [] { piece });

                var state = GameState.New(game, null);
                var innerRule1 = new SimpleRule(
                    RuleCheckState.Valid, 
                    x => x.Update(new PieceState(piece, board.GetTile("tile-1"))));

                var rule = new CompositeRule(new[] { innerRule1 }); 

                // act
                var actual = rule.Evaluate(game, state, new NullEvent());
                
                // assert
                Assert.Equal(board.GetTile("tile-1"), actual.GetState<PieceState>(piece).CurrentTile);
                Assert.Equal(1, innerRule1.CheckCallCount);
                Assert.Equal(1, innerRule1.EvaluateCallCount); // to properly chain... evaluate is called after successfull check
            }

            [Fact]
            public void Should_return_original_state_invalid()
            {
                // arrange
                var player = new Player("player");
                var board = new TestBoard();
                var piece = new Piece("id", player, new[] { new DirectionPattern(Direction.CounterClockwise) });
                var game = new Game(
                    "game",
                    board,
                    new [] { player },
                    new [] { piece });

                var state = GameState.New(game, null);
                var innerRule1 = new SimpleRule(
                    RuleCheckState.Invalid, 
                    x => x.Update(new PieceState(piece, board.GetTile("tile-1"))));

                var rule = new CompositeRule(new[] { innerRule1 }); 

                // act
                var actual = rule.Evaluate(game, state, new NullEvent());
                
                // assert
                Assert.Equal(state, actual);
                Assert.Equal(1, innerRule1.CheckCallCount);
                Assert.Equal(0, innerRule1.EvaluateCallCount); // to properly chain... evaluate is called after successfull check
            }

            [Fact]
            public void Should_return_chained_result()
            {
                // arrange
                var player = new Player("player");
                var board = new TestBoard();
                var piece = new Piece("id", player, new[] { new DirectionPattern(Direction.CounterClockwise) });
                var game = new Game(
                    "game",
                    board,
                    new [] { player },
                    new [] { piece });
                var die = new RegularDie("die");
                    
                var state = GameState.New(game, null);
                var innerRule1 = new SimpleRule(
                    RuleCheckState.Valid, 
                    x => x.Update(new PieceState(piece, board.GetTile("tile-1"))));
                var innerRule2 = new SimpleRule(
                    RuleCheckState.Valid, 
                    x => x.Update(new PieceState(piece, board.GetTile("tile-2"))));
                var innerRule3 = new SimpleRule(
                    RuleCheckState.Valid, 
                    x => x.Update(new DieState<int>(die, 4)));

                var rule = new CompositeRule(new[] { innerRule1, innerRule2, innerRule3 }); 

                // act
                var actual = rule.Evaluate(game, state, new NullEvent());
                
                // assert
                Assert.Equal(2, actual.ChildStates.Count());
                Assert.Equal(board.GetTile("tile-2"), actual.GetState<PieceState>(piece).CurrentTile);
                Assert.Equal(4, actual.GetState<DieState<int>>(die).CurrentValue);
                Assert.Equal(1, innerRule1.CheckCallCount);
                Assert.Equal(1, innerRule1.EvaluateCallCount); // to properly chain... evaluate is called after successfull check
                Assert.Equal(1, innerRule2.CheckCallCount);
                Assert.Equal(1, innerRule2.EvaluateCallCount); // to properly chain... evaluate is called after successfull check
                Assert.Equal(1, innerRule3.CheckCallCount);
                Assert.Equal(1, innerRule3.EvaluateCallCount); // to properly chain... evaluate is called after successfull check
            }

            [Fact]
            public void Should_return_original_state_not_applicable()
            {
                // arrange
                var player = new Player("player");
                var board = new TestBoard();
                var piece = new Piece("id", player, new[] { new DirectionPattern(Direction.CounterClockwise) });
                var game = new Game(
                    "game",
                    board,
                    new [] { player },
                    new [] { piece });
                var die = new RegularDie("die");
                    
                var state = GameState.New(game, null);
                var innerRule1 = new SimpleRule(
                    RuleCheckState.NotApplicable, 
                    x => x.Update(new PieceState(piece, board.GetTile("tile-1"))));

                var rule = new CompositeRule(new[] { innerRule1 }); 

                // act
                var actual = rule.Evaluate(game, state, new NullEvent());
                
                // assert
                Assert.Equal(state, actual);
                Assert.Equal(1, innerRule1.CheckCallCount);
                Assert.Equal(0, innerRule1.EvaluateCallCount); // to properly chain... evaluate is called after successfull check
            }

            [Fact]
            public void Should_return_partly_chained_result()
            {
                // arrange
                var player = new Player("player");
                var board = new TestBoard();
                var piece = new Piece("id", player, new[] { new DirectionPattern(Direction.CounterClockwise) });
                var game = new Game(
                    "game",
                    board,
                    new [] { player },
                    new [] { piece });
                var die = new RegularDie("die");
                    
                var state = GameState.New(game, null);
                var innerRule1 = new SimpleRule(
                    RuleCheckState.Valid, 
                    x => x.Update(new PieceState(piece, board.GetTile("tile-1"))));
                var innerRule2 = new SimpleRule(
                    RuleCheckState.NotApplicable, 
                    x => x.Update(new PieceState(piece, board.GetTile("tile-2"))));
                var innerRule3 = new SimpleRule(
                    RuleCheckState.Valid, 
                    x => x.Update(new DieState<int>(die, 4)));

                var rule = new CompositeRule(new[] { innerRule1, innerRule2, innerRule3 }); 

                // act
                var actual = rule.Evaluate(game, state, new NullEvent());
                
                // assert
                Assert.Equal(2, actual.ChildStates.Count());
                Assert.Equal(board.GetTile("tile-1"), actual.GetState<PieceState>(piece).CurrentTile);
                Assert.Equal(4, actual.GetState<DieState<int>>(die).CurrentValue);
                Assert.Equal(1, innerRule1.CheckCallCount);
                Assert.Equal(1, innerRule1.EvaluateCallCount); // to properly chain... evaluate is called after successfull check
                Assert.Equal(1, innerRule2.CheckCallCount);
                Assert.Equal(0, innerRule2.EvaluateCallCount); // to properly chain... evaluate is called after successfull check
                Assert.Equal(1, innerRule3.CheckCallCount);
                Assert.Equal(1, innerRule3.EvaluateCallCount); // to properly chain... evaluate is called after successfull check
            }
        }
    }
}