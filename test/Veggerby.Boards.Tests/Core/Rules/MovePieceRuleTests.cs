using Shouldly;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Artifacts.Patterns;
using Veggerby.Boards.Core.Events;
using Veggerby.Boards.Core.Phases;
using Veggerby.Boards.Core.Rules;
using Veggerby.Boards.Core.States;
using Veggerby.Boards.Tests.Core.Fakes;
using Xunit;

namespace Veggerby.Boards.Tests.Core.Rules
{
    public class MovePieceRuleTests
    {
        public class Check
        {
            [Fact]
            public void Should_return_valid_simple_move()
            {
                // arrange
                var player1 = new Player("player-1");
                var player2 = new Player("player-2");
                var piece1 = new Piece("piece-1", player1, new [] { new AnyPattern() });
                var piece2 = new Piece("piece-2", player2, new [] { new AnyPattern() });
                var board = new TestBoard();
                var game = new Game("game", board, new [] { player1, player2 }, new[] { piece1, piece2 });

                var state = GameState.New(game, new IState[] {
                    new TurnState(player1, new Turn(new Round(1), 1)),
                    new PieceState(piece1, board.GetTile("tile-1")),
                    new PieceState(piece2, board.GetTile("tile-4"))
                } );

                var rule = new SimpleMovePieceRule();

                var @event = new MovePieceGameEvent(piece1, board.GetTile("tile-1"), board.GetTile("tile-3"));

                // act
                var actual = rule.Check(game, state, @event);

                // assert
                actual.ShouldBe(RuleCheckState.Valid);
            }

            [Fact]
            public void Should_return_invalid_wrong_from()
            {
                // arrange
                var player1 = new Player("player-1");
                var player2 = new Player("player-2");
                var piece1 = new Piece("piece-1", player1, new [] { new AnyPattern() });
                var piece2 = new Piece("piece-2", player2, new [] { new AnyPattern() });
                var board = new TestBoard();
                var game = new Game("game", board, new [] { player1, player2 }, new[] { piece1, piece2 });

                var state = GameState.New(game, new IState[] {
                    new TurnState(player1, new Turn(new Round(1), 1)),
                    new PieceState(piece1, board.GetTile("tile-1")),
                    new PieceState(piece2, board.GetTile("tile-4"))
                } );

                var rule = new SimpleMovePieceRule();

                var @event = new MovePieceGameEvent(piece1, board.GetTile("tile-2"), board.GetTile("tile-3"));

                // act
                var actual = rule.Check(game, state, @event);

                // assert
                actual.ShouldBe(RuleCheckState.Invalid);
                actual.Reason.ShouldBe("InvalidEventFrom");
            }

            [Fact]
            public void Should_return_invalid_wrong_turn()
            {
                // arrange
                var player1 = new Player("player-1");
                var player2 = new Player("player-2");
                var piece1 = new Piece("piece-1", player1, new [] { new AnyPattern() });
                var piece2 = new Piece("piece-2", player2, new [] { new AnyPattern() });
                var board = new TestBoard();
                var game = new Game("game", board, new [] { player1, player2 }, new[] { piece1, piece2 });

                var state = GameState.New(game, new IState[] {
                    new TurnState(player1, new Turn(new Round(1), 1)),
                    new PieceState(piece1, board.GetTile("tile-1")),
                    new PieceState(piece2, board.GetTile("tile-4"))
                } );

                var rule = new SimpleMovePieceRule();

                var @event = new MovePieceGameEvent(piece2, board.GetTile("tile-4"), board.GetTile("tile-10"));

                // act
                var actual = rule.Check(game, state, @event);

                // assert
                actual.ShouldBe(RuleCheckState.Invalid);
                actual.Reason.ShouldBe("InvalidTurn");
            }

            [Fact]
            public void Should_return_invalid_no_active_turn()
            {
                // arrange
                var player1 = new Player("player-1");
                var player2 = new Player("player-2");
                var piece1 = new Piece("piece-1", player1, new [] { new AnyPattern() });
                var piece2 = new Piece("piece-2", player2, new [] { new AnyPattern() });
                var board = new TestBoard();
                var game = new Game("game", board, new [] { player1, player2 }, new[] { piece1, piece2 });

                var state = GameState.New(game, new IState[] {
                    new PieceState(piece1, board.GetTile("tile-1")),
                    new PieceState(piece2, board.GetTile("tile-4"))
                } );

                var rule = new SimpleMovePieceRule();

                var @event = new MovePieceGameEvent(piece1, board.GetTile("tile-1"), board.GetTile("tile-10"));

                // act
                var actual = rule.Check(game, state, @event);

                // assert
                actual.ShouldBe(RuleCheckState.Invalid);
                actual.Reason.ShouldBe("InvalidTurn");
            }

            [Fact]
            public void Should_return_invalid_no_piece_state()
            {
                // arrange
                var player1 = new Player("player-1");
                var player2 = new Player("player-2");
                var piece1 = new Piece("piece-1", player1, new [] { new AnyPattern() });
                var piece2 = new Piece("piece-2", player2, new [] { new AnyPattern() });
                var board = new TestBoard();
                var game = new Game("game", board, new [] { player1, player2 }, new[] { piece1, piece2 });

                var state = GameState.New(game, new IState[] {
                    new TurnState(player1, new Turn(new Round(1), 1)),
                    new PieceState(piece2, board.GetTile("tile-4"))
                } );

                var rule = new SimpleMovePieceRule();

                var @event = new MovePieceGameEvent(piece1, board.GetTile("tile-1"), board.GetTile("tile-10"));

                // act
                var actual = rule.Check(game, state, @event);

                // assert
                actual.ShouldBe(RuleCheckState.Invalid);
                actual.Reason.ShouldBe("InvalidPiece");
            }

            [Fact]
            public void Should_return_invalid_wrong_path_from()
            {
                // arrange
                var player1 = new Player("player-1");
                var player2 = new Player("player-2");
                var piece1 = new Piece("piece-1", player1, new [] { new AnyPattern() });
                var piece2 = new Piece("piece-2", player2, new [] { new AnyPattern() });
                var board = new TestBoard();
                var game = new Game("game", board, new [] { player1, player2 }, new[] { piece1, piece2 });

                var state = GameState.New(game, new IState[] {
                    new TurnState(player1, new Turn(new Round(1), 1)),
                    new PieceState(piece1, board.GetTile("tile-1")),
                    new PieceState(piece2, board.GetTile("tile-4"))
                } );

                // return path tile-2 --> tile-10
                var rule = new SimpleMovePieceRule(board.GetTile("tile-2"));

                var @event = new MovePieceGameEvent(piece1, board.GetTile("tile-1"), board.GetTile("tile-10"));

                // act
                var actual = rule.Check(game, state, @event);

                // assert
                actual.ShouldBe(RuleCheckState.Invalid);
                actual.Reason.ShouldBe("InvalidPath");
            }

            [Fact]
            public void Should_return_invalid_wrong_path_to()
            {
                // arrange
                var player1 = new Player("player-1");
                var player2 = new Player("player-2");
                var piece1 = new Piece("piece-1", player1, new [] { new AnyPattern() });
                var piece2 = new Piece("piece-2", player2, new [] { new AnyPattern() });
                var board = new TestBoard();
                var game = new Game("game", board, new [] { player1, player2 }, new[] { piece1, piece2 });

                var state = GameState.New(game, new IState[] {
                    new TurnState(player1, new Turn(new Round(1), 1)),
                    new PieceState(piece1, board.GetTile("tile-1")),
                    new PieceState(piece2, board.GetTile("tile-4"))
                } );

                // return path tile-1 --> tile-8
                var rule = new SimpleMovePieceRule(null, board.GetTile("tile-8"));

                var @event = new MovePieceGameEvent(piece1, board.GetTile("tile-1"), board.GetTile("tile-10"));

                // act
                var actual = rule.Check(game, state, @event);

                // assert
                actual.ShouldBe(RuleCheckState.Invalid);
                actual.Reason.ShouldBe("InvalidPath");
            }

            [Fact]
            public void Should_return_invalid_can_move_path_is_false()
            {
                // arrange
                var player1 = new Player("player-1");
                var player2 = new Player("player-2");
                var piece1 = new Piece("piece-1", player1, new [] { new AnyPattern() });
                var piece2 = new Piece("piece-2", player2, new [] { new AnyPattern() });
                var board = new TestBoard();
                var game = new Game("game", board, new [] { player1, player2 }, new[] { piece1, piece2 });

                var state = GameState.New(game, new IState[] {
                    new TurnState(player1, new Turn(new Round(1), 1)),
                    new PieceState(piece1, board.GetTile("tile-1")),
                    new PieceState(piece2, board.GetTile("tile-4"))
                } );

                // return path tile-1 --> tile-8
                var rule = new SimpleMovePieceRule(null, null, null, false);

                var @event = new MovePieceGameEvent(piece1, board.GetTile("tile-1"), board.GetTile("tile-10"));

                // act
                var actual = rule.Check(game, state, @event);

                // assert
                actual.ShouldBe(RuleCheckState.Invalid);
                actual.Reason.ShouldBe("NoMovePath");
            }
        }

        public class Evaluate
        {
            [Fact]
            public void Should_move_piece()
            {
                // arrange
                // arrange
                var player1 = new Player("player-1");
                var player2 = new Player("player-2");
                var piece1 = new Piece("piece-1", player1, new [] { new AnyPattern() });
                var piece2 = new Piece("piece-2", player2, new [] { new AnyPattern() });
                var board = new TestBoard();
                var game = new Game("game", board, new [] { player1, player2 }, new[] { piece1, piece2 });

                var state = GameState.New(game, new IState[] {
                    new TurnState(player1, new Turn(new Round(1), 1)),
                    new PieceState(piece1, board.GetTile("tile-1")),
                    new PieceState(piece2, board.GetTile("tile-4"))
                } );

                var rule = new SimpleMovePieceRule();

                var @event = new MovePieceGameEvent(piece1, board.GetTile("tile-1"), board.GetTile("tile-3"));
                
                // act
                var actual = rule.Evaluate(game, state, @event);
                
                // assert
                var piece1State = actual.GetState<PieceState>(piece1);
                var piece2State = actual.GetState<PieceState>(piece2);
                piece1State.Artifact.ShouldBe(piece1);
                piece1State.CurrentTile.ShouldBe(board.GetTile("tile-3"));
                piece2State.Artifact.ShouldBe(piece2);
                piece2State.CurrentTile.ShouldBe(board.GetTile("tile-4"));
            }
        }
    }
}