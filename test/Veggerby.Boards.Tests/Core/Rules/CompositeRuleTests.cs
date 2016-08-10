using System.Linq;
using Shouldly;
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
                actual.ShouldBe(RuleCheckState.Valid);
                innerRule1.CheckCallCount.ShouldBe(1);
                innerRule1.EvaluateCallCount.ShouldBe(1); // to properly chain... evaluate is called after successfull check
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
                actual.ShouldBe(RuleCheckState.Valid);
                innerRule1.CheckCallCount.ShouldBe(1);
                innerRule1.EvaluateCallCount.ShouldBe(1); // to properly chain... evaluate is called after successfull check
                innerRule2.CheckCallCount.ShouldBe(1);
                innerRule2.EvaluateCallCount.ShouldBe(1); // to properly chain... evaluate is called after successfull check
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
                var innerRule2 = new SimpleRule(RuleCheckState.Fail("Rule2"));

                var rule = new CompositeRule(new[] { innerRule1, innerRule2 }); 

                // act
                var actual = rule.Check(game, GameState.New(game, null), new NullEvent());
                
                // assert
                actual.ShouldBe(RuleCheckState.Invalid);
                actual.Reason.ShouldBe("Rule2");
                innerRule1.CheckCallCount.ShouldBe(1);
                innerRule1.EvaluateCallCount.ShouldBe(1); // to properly chain... evaluate is called after successfull check
                innerRule2.CheckCallCount.ShouldBe(1);
                innerRule2.EvaluateCallCount.ShouldBe(0); // to properly chain... evaluate is called after successfull check
            }


            [Fact]
            public void Should_return_simple_result_multiple_more_invalid()
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
                var innerRule2 = new SimpleRule(RuleCheckState.Fail("Rule2"));
                var innerRule3 = new SimpleRule(RuleCheckState.Fail("Rule3"));

                var rule = new CompositeRule(new[] { innerRule1, innerRule2, innerRule3 }); 

                // act
                var actual = rule.Check(game, GameState.New(game, null), new NullEvent());
                
                // assert
                actual.ShouldBe(RuleCheckState.Invalid);
                actual.Reason.ShouldBe("Rule2,Rule3");
                innerRule1.CheckCallCount.ShouldBe(1);
                innerRule1.EvaluateCallCount.ShouldBe(1); // to properly chain... evaluate is called after successfull check
                innerRule2.CheckCallCount.ShouldBe(1);
                innerRule2.EvaluateCallCount.ShouldBe(0); // to properly chain... evaluate is called after successfull check
                innerRule3.CheckCallCount.ShouldBe(1);
                innerRule3.EvaluateCallCount.ShouldBe(0); // to properly chain... evaluate is called after successfull check
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
                actual.ShouldBe(RuleCheckState.Valid);
                innerRule1.CheckCallCount.ShouldBe(1);
                innerRule1.EvaluateCallCount.ShouldBe(0); // to properly chain... evaluate is called after successfull check
                innerRule2.CheckCallCount.ShouldBe(1);
                innerRule2.EvaluateCallCount.ShouldBe(1); // to properly chain... evaluate is called after successfull check
                innerRule3.CheckCallCount.ShouldBe(1);
                innerRule3.EvaluateCallCount.ShouldBe(1); // to properly chain... evaluate is called after successfull check
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
                actual.ShouldBe(RuleCheckState.Valid);
                innerRule1.CheckCallCount.ShouldBe(1);
                innerRule1.EvaluateCallCount.ShouldBe(0); // to properly chain... evaluate is called after successfull check
                innerRule2.CheckCallCount.ShouldBe(1);
                innerRule2.EvaluateCallCount.ShouldBe(1); // to properly chain... evaluate is called after successfull check
                innerRule3.CheckCallCount.ShouldBe(0);
                innerRule3.EvaluateCallCount.ShouldBe(0); // to properly chain... evaluate is called after successfull check
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
                actual.ShouldBe(RuleCheckState.Valid);
                innerRule1.CheckCallCount.ShouldBe(1);
                innerRule1.EvaluateCallCount.ShouldBe(0); // to properly chain... evaluate is called after successfull check
                innerRule2.CheckCallCount.ShouldBe(1);
                innerRule2.EvaluateCallCount.ShouldBe(1); // to properly chain... evaluate is called after successfull check
                innerRule3.CheckCallCount.ShouldBe(0);
                innerRule3.EvaluateCallCount.ShouldBe(0); // to properly chain... evaluate is called after successfull check
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
                actual.GetState<PieceState>(piece).CurrentTile.ShouldBe(board.GetTile("tile-1"));
                innerRule1.CheckCallCount.ShouldBe(1);
                innerRule1.EvaluateCallCount.ShouldBe(1); // to properly chain... evaluate is called after successfull check
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
                actual.ShouldBe(state);
                innerRule1.CheckCallCount.ShouldBe(1);
                innerRule1.EvaluateCallCount.ShouldBe(0); // to properly chain... evaluate is called after successfull check
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
                actual.ChildStates.Count().ShouldBe(2);
                actual.GetState<PieceState>(piece).CurrentTile.ShouldBe(board.GetTile("tile-2"));
                actual.GetState<DieState<int>>(die).CurrentValue.ShouldBe(4);
                innerRule1.CheckCallCount.ShouldBe(1);
                innerRule1.EvaluateCallCount.ShouldBe(1); // to properly chain... evaluate is called after successfull check
                innerRule2.CheckCallCount.ShouldBe(1);
                innerRule2.EvaluateCallCount.ShouldBe(1); // to properly chain... evaluate is called after successfull check
                innerRule3.CheckCallCount.ShouldBe(1);
                innerRule3.EvaluateCallCount.ShouldBe(1); // to properly chain... evaluate is called after successfull check
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
                actual.ShouldBe(state);
                innerRule1.CheckCallCount.ShouldBe(1);
                innerRule1.EvaluateCallCount.ShouldBe(0); // to properly chain... evaluate is called after successfull check
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
                actual.ChildStates.Count().ShouldBe(2);
                actual.GetState<PieceState>(piece).CurrentTile.ShouldBe(board.GetTile("tile-1"));
                actual.GetState<DieState<int>>(die).CurrentValue.ShouldBe(4);
                innerRule1.CheckCallCount.ShouldBe(1);
                innerRule1.EvaluateCallCount.ShouldBe(1); // to properly chain... evaluate is called after successfull check
                innerRule2.CheckCallCount.ShouldBe(1);
                innerRule2.EvaluateCallCount.ShouldBe(0); // to properly chain... evaluate is called after successfull check
                innerRule3.CheckCallCount.ShouldBe(1);
                innerRule3.EvaluateCallCount.ShouldBe(1); // to properly chain... evaluate is called after successfull check
            }
        }
    }
}