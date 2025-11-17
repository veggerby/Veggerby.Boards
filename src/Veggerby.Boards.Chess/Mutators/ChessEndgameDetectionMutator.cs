using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Chess.MoveGeneration;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Chess.Mutators;

/// <summary>
/// Detects checkmate and stalemate conditions after a move and adds terminal states if applicable.
/// </summary>
/// <remarks>
/// This mutator runs after a move is completed to check if the game has reached a terminal state.
/// If checkmate or stalemate is detected, it adds both <see cref="GameEndedState"/> and 
/// <see cref="ChessOutcomeState"/> to mark the game as complete with outcome details.
/// </remarks>
public sealed class ChessEndgameDetectionMutator : IStateMutator<MovePieceGameEvent>
{
    private readonly ChessLegalityFilter _legalityFilter;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChessEndgameDetectionMutator"/> class.
    /// </summary>
    /// <param name="game">The chess game definition.</param>
    public ChessEndgameDetectionMutator(Game game)
    {
        ArgumentNullException.ThrowIfNull(game);
        _legalityFilter = new ChessLegalityFilter(game);
    }

    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState state, MovePieceGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(@event);

        // Skip if game already ended
        if (state.GetStates<GameEndedState>().Any())
        {
            return state;
        }

        // Get the active player after the move
        if (!state.TryGetActivePlayer(out var activePlayer) || activePlayer == null)
        {
            return state;
        }

        // Check if there are any legal moves
        var legalMoves = _legalityFilter.GenerateLegalMoves(state);

        if (legalMoves.Count > 0)
        {
            // Game continues
            return state;
        }

        // No legal moves - determine if checkmate or stalemate
        var inCheck = IsInCheck(state, activePlayer, engine.Game);

        if (inCheck)
        {
            // Checkmate - the player who just moved is the winner
            var winner = GetOpponent(state, activePlayer, engine.Game);
            var outcomeState = new ChessOutcomeState(EndgameStatus.Checkmate, winner);
            return state.Next(new IArtifactState[] { new GameEndedState(), outcomeState });
        }
        else
        {
            // Stalemate - draw
            var outcomeState = new ChessOutcomeState(EndgameStatus.Stalemate, null);
            return state.Next(new IArtifactState[] { new GameEndedState(), outcomeState });
        }
    }

    private static bool IsInCheck(GameState state, Player player, Game game)
    {
        // Find the king
        var kingPiece = FindKing(state, player);

        if (kingPiece == null)
        {
            return false;
        }

        var kingState = state.GetState<PieceState>(kingPiece);

        if (kingState == null || kingState.CurrentTile == null)
        {
            return false;
        }

        // Check if any opponent piece can attack the king
        var pieceStates = state.GetStates<PieceState>();

        foreach (var pieceState in pieceStates)
        {
            if (state.IsCaptured(pieceState.Artifact))
            {
                continue;
            }

            if (pieceState.Artifact.Owner?.Id == player.Id)
            {
                continue;
            }

            // Check if this opponent piece can attack the king's square
            if (CanAttackSquare(state, pieceState, kingState.CurrentTile, game))
            {
                return true;
            }
        }

        return false;
    }

    private static bool CanAttackSquare(GameState state, PieceState attackerState, Tile targetSquare, Game game)
    {
        var piece = attackerState.Artifact;
        var from = attackerState.CurrentTile;

        if (from == null)
        {
            return false;
        }

        return CheckAttackPattern(state, piece, from, targetSquare, game);
    }

    private static bool CheckAttackPattern(GameState state, Piece piece, Tile from, Tile targetSquare, Game game)
    {
        if (ChessPiece.IsPawn(state, piece.Id))
        {
            return CanPawnAttack(state, piece, from, targetSquare, game);
        }
        else if (ChessPiece.IsKnight(state, piece.Id))
        {
            return CanKnightAttack(from, targetSquare, game);
        }
        else if (ChessPiece.IsBishop(state, piece.Id) || ChessPiece.IsQueen(state, piece.Id))
        {
            if (CanSlidingAttack(state, from, targetSquare, game, diagonal: true))
            {
                return true;
            }
        }

        if (ChessPiece.IsRook(state, piece.Id) || ChessPiece.IsQueen(state, piece.Id))
        {
            if (CanSlidingAttack(state, from, targetSquare, game, orthogonal: true))
            {
                return true;
            }
        }

        if (ChessPiece.IsKing(state, piece.Id))
        {
            return CanKingAttack(from, targetSquare, game);
        }

        return false;
    }

    private static bool CanPawnAttack(GameState state, Piece pawn, Tile from, Tile targetSquare, Game game)
    {
        var isWhite = ChessPiece.IsWhite(state, pawn.Id);
        var diagonalDirections = isWhite
            ? new[] { Constants.Directions.NorthEast, Constants.Directions.NorthWest }
            : new[] { Constants.Directions.SouthEast, Constants.Directions.SouthWest };

        foreach (var diagDir in diagonalDirections)
        {
            var diagonal = GetRelatedTile(from, diagDir, game);

            if (diagonal != null && diagonal.Equals(targetSquare))
            {
                return true;
            }
        }

        return false;
    }

    private static bool CanKnightAttack(Tile from, Tile targetSquare, Game game)
    {
        var knightOffsets = new[]
        {
            (Constants.Directions.North, Constants.Directions.North, Constants.Directions.East),
            (Constants.Directions.North, Constants.Directions.North, Constants.Directions.West),
            (Constants.Directions.South, Constants.Directions.South, Constants.Directions.East),
            (Constants.Directions.South, Constants.Directions.South, Constants.Directions.West),
            (Constants.Directions.East, Constants.Directions.East, Constants.Directions.North),
            (Constants.Directions.East, Constants.Directions.East, Constants.Directions.South),
            (Constants.Directions.West, Constants.Directions.West, Constants.Directions.North),
            (Constants.Directions.West, Constants.Directions.West, Constants.Directions.South)
        };

        foreach (var (first, second, third) in knightOffsets)
        {
            var intermediate1 = GetRelatedTile(from, first, game);
            if (intermediate1 == null) continue;

            var intermediate2 = GetRelatedTile(intermediate1, second, game);
            if (intermediate2 == null) continue;

            var target = GetRelatedTile(intermediate2, third, game);
            if (target != null && target.Equals(targetSquare))
            {
                return true;
            }
        }

        return false;
    }

    private static bool CanSlidingAttack(GameState state, Tile from, Tile targetSquare, Game game, bool diagonal = false, bool orthogonal = false)
    {
        var directions = new System.Collections.Generic.List<string>();

        if (diagonal)
        {
            directions.Add(Constants.Directions.NorthEast);
            directions.Add(Constants.Directions.NorthWest);
            directions.Add(Constants.Directions.SouthEast);
            directions.Add(Constants.Directions.SouthWest);
        }

        if (orthogonal)
        {
            directions.Add(Constants.Directions.North);
            directions.Add(Constants.Directions.South);
            directions.Add(Constants.Directions.East);
            directions.Add(Constants.Directions.West);
        }

        foreach (var direction in directions)
        {
            var current = from;

            while (true)
            {
                var next = GetRelatedTile(current, direction, game);

                if (next == null)
                {
                    break;
                }

                if (next.Equals(targetSquare))
                {
                    return true;
                }

                if (state.GetPiecesOnTile(next).Any())
                {
                    break;
                }

                current = next;
            }
        }

        return false;
    }

    private static bool CanKingAttack(Tile from, Tile targetSquare, Game game)
    {
        var directions = new[]
        {
            Constants.Directions.North,
            Constants.Directions.South,
            Constants.Directions.East,
            Constants.Directions.West,
            Constants.Directions.NorthEast,
            Constants.Directions.NorthWest,
            Constants.Directions.SouthEast,
            Constants.Directions.SouthWest
        };

        foreach (var direction in directions)
        {
            var target = GetRelatedTile(from, direction, game);

            if (target != null && target.Equals(targetSquare))
            {
                return true;
            }
        }

        return false;
    }

    private static Piece? FindKing(GameState state, Player player)
    {
        var pieceStates = state.GetStates<PieceState>();

        foreach (var pieceState in pieceStates)
        {
            if (pieceState.Artifact.Owner?.Id == player.Id &&
                ChessPiece.IsKing(state, pieceState.Artifact.Id) &&
                !state.IsCaptured(pieceState.Artifact))
            {
                return pieceState.Artifact;
            }
        }

        return null;
    }

    private static Player? GetOpponent(GameState state, Player currentPlayer, Game game)
    {
        foreach (var player in game.Players)
        {
            if (player.Id != currentPlayer.Id)
            {
                return player;
            }
        }

        return null;
    }

    private static Tile? GetRelatedTile(Tile from, string direction, Game game)
    {
        var relations = game.Board.TileRelations
            .Where(r => r.From == from && r.Direction.Id == direction);

        foreach (var relation in relations)
        {
            return relation.To;
        }

        return null;
    }
}
