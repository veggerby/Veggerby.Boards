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
                Assert.Equal(RuleCheckState.Valid, actual);
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
                Assert.Equal(RuleCheckState.Invalid, actual);
                Assert.Equal("InvalidEventFrom", actual.Reason);
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
                Assert.Equal(RuleCheckState.Invalid, actual);
                Assert.Equal("InvalidTurn", actual.Reason);
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
                Assert.Equal(RuleCheckState.Invalid, actual);
                Assert.Equal("InvalidTurn", actual.Reason);
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
                Assert.Equal(RuleCheckState.Invalid, actual);
                Assert.Equal("InvalidPiece", actual.Reason);
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
                Assert.Equal(RuleCheckState.Invalid, actual);
                Assert.Equal("InvalidPath", actual.Reason);
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
                Assert.Equal(RuleCheckState.Invalid, actual);
                Assert.Equal("InvalidPath", actual.Reason);
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
                Assert.Equal(RuleCheckState.Invalid, actual);
                Assert.Equal("NoMovePath", actual.Reason);
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
                Assert.Equal(piece1, piece1State.Artifact);
                Assert.Equal(board.GetTile("tile-3"), piece1State.CurrentTile);
                Assert.Equal(piece2, piece2State.Artifact);
                Assert.Equal(piece1, piece1State.Artifact);
                Assert.Equal(board.GetTile("tile-4"), piece2State.CurrentTile);
            }
        }
    }
}