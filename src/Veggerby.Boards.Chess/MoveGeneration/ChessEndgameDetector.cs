using System;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Chess.MoveGeneration;

/// <summary>
/// Detects endgame conditions in chess (checkmate, stalemate).
/// </summary>
public sealed class ChessEndgameDetector
{
    private readonly Game _game;
    private readonly ChessLegalityFilter _legalityFilter;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChessEndgameDetector"/> class.
    /// </summary>
    /// <param name="game">The chess game definition.</param>
    public ChessEndgameDetector(Game game)
    {
        _game = game ?? throw new ArgumentNullException(nameof(game));
        _legalityFilter = new ChessLegalityFilter(game);
    }

    /// <summary>
    /// Checks if the current position is checkmate.
    /// </summary>
    /// <param name="state">Current game state.</param>
    /// <returns>True if the active player is in checkmate.</returns>
    public bool IsCheckmate(GameState state)
    {
        if (state == null)
            throw new ArgumentNullException(nameof(state));

        if (!state.TryGetActivePlayer(out var activePlayer) || activePlayer == null)
        {
            return false;
        }

        // Checkmate requires: 
        // 1. The king is in check
        // 2. There are no legal moves
        var legalMoves = _legalityFilter.GenerateLegalMoves(state);

        if (legalMoves.Count > 0)
        {
            return false;
        }

        // No legal moves - need to check if king is in check
        return IsInCheck(state, activePlayer);
    }

    /// <summary>
    /// Checks if the current position is stalemate.
    /// </summary>
    /// <param name="state">Current game state.</param>
    /// <returns>True if the active player is in stalemate.</returns>
    public bool IsStalemate(GameState state)
    {
        if (state == null)
            throw new ArgumentNullException(nameof(state));

        if (!state.TryGetActivePlayer(out var activePlayer) || activePlayer == null)
        {
            return false;
        }

        // Stalemate requires:
        // 1. The king is NOT in check
        // 2. There are no legal moves
        var legalMoves = _legalityFilter.GenerateLegalMoves(state);

        if (legalMoves.Count > 0)
        {
            return false;
        }

        // No legal moves - need to check if king is NOT in check
        return !IsInCheck(state, activePlayer);
    }

    /// <summary>
    /// Checks if the current position is an endgame (checkmate or stalemate).
    /// </summary>
    /// <param name="state">Current game state.</param>
    /// <returns>True if the game has ended.</returns>
    public bool IsGameOver(GameState state)
    {
        return IsCheckmate(state) || IsStalemate(state);
    }

    /// <summary>
    /// Gets the endgame status of the current position.
    /// </summary>
    /// <param name="state">Current game state.</param>
    /// <returns>The endgame status.</returns>
    public EndgameStatus GetEndgameStatus(GameState state)
    {
        if (IsCheckmate(state))
        {
            return EndgameStatus.Checkmate;
        }

        if (IsStalemate(state))
        {
            return EndgameStatus.Stalemate;
        }

        if (!state.TryGetActivePlayer(out var activePlayer) || activePlayer == null)
        {
            return EndgameStatus.InProgress;
        }

        if (IsInCheck(state, activePlayer))
        {
            return EndgameStatus.Check;
        }

        return EndgameStatus.InProgress;
    }

    private bool IsInCheck(GameState state, Player player)
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

            // Generate pseudo-legal moves for this opponent piece
            var moveGenerator = new ChessMoveGenerator(_game);

            // Create a temporary state with the opponent as active player to generate their moves
            // We just need to check if any of their moves target the king's square
            if (CanAttackSquare(state, pieceState, kingState.CurrentTile))
            {
                return true;
            }
        }

        return false;
    }

    private bool CanAttackSquare(GameState state, PieceState attackerState, Tile targetSquare)
    {
        var piece = attackerState.Artifact;
        var from = attackerState.CurrentTile;

        if (from == null)
        {
            return false;
        }

        // Use the same attack logic as the legality filter
        var filter = new ChessLegalityFilter(_game);

        // We need to check if the piece can attack the target square
        // For simplicity, we'll check if a pseudo-legal move to that square exists
        // Note: This is a simplified check - in reality we'd need to verify the attack pattern

        return CheckAttackPattern(state, piece, from, targetSquare);
    }

    private bool CheckAttackPattern(GameState state, Piece piece, Tile from, Tile targetSquare)
    {
        // This is a simplified attack check - delegates to helper methods
        if (ChessPiece.IsPawn(state, piece.Id))
        {
            return CanPawnAttack(state, piece, from, targetSquare);
        }
        else if (ChessPiece.IsKnight(state, piece.Id))
        {
            return CanKnightAttack(from, targetSquare);
        }
        else if (ChessPiece.IsBishop(state, piece.Id) || ChessPiece.IsQueen(state, piece.Id))
        {
            if (CanSlidingAttack(state, from, targetSquare, diagonal: true))
            {
                return true;
            }
        }

        if (ChessPiece.IsRook(state, piece.Id) || ChessPiece.IsQueen(state, piece.Id))
        {
            if (CanSlidingAttack(state, from, targetSquare, orthogonal: true))
            {
                return true;
            }
        }

        if (ChessPiece.IsKing(state, piece.Id))
        {
            return CanKingAttack(from, targetSquare);
        }

        return false;
    }

    private bool CanPawnAttack(GameState state, Piece pawn, Tile from, Tile targetSquare)
    {
        var isWhite = ChessPiece.IsWhite(state, pawn.Id);
        var diagonalDirections = isWhite
            ? new[] { Constants.Directions.NorthEast, Constants.Directions.NorthWest }
            : new[] { Constants.Directions.SouthEast, Constants.Directions.SouthWest };

        foreach (var diagDir in diagonalDirections)
        {
            var diagonal = GetRelatedTile(from, diagDir);

            if (diagonal != null && diagonal.Equals(targetSquare))
            {
                return true;
            }
        }

        return false;
    }

    private bool CanKnightAttack(Tile from, Tile targetSquare)
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
            var intermediate1 = GetRelatedTile(from, first);
            if (intermediate1 == null)
                continue;

            var intermediate2 = GetRelatedTile(intermediate1, second);
            if (intermediate2 == null)
                continue;

            var target = GetRelatedTile(intermediate2, third);
            if (target != null && target.Equals(targetSquare))
            {
                return true;
            }
        }

        return false;
    }

    private bool CanSlidingAttack(GameState state, Tile from, Tile targetSquare, bool diagonal = false, bool orthogonal = false)
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
                var next = GetRelatedTile(current, direction);

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

    private bool CanKingAttack(Tile from, Tile targetSquare)
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
            var target = GetRelatedTile(from, direction);

            if (target != null && target.Equals(targetSquare))
            {
                return true;
            }
        }

        return false;
    }

    private Piece? FindKing(GameState state, Player player)
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

    private Tile? GetRelatedTile(Tile from, string direction)
    {
        var relations = _game.Board.TileRelations
            .Where(r => r.From == from && r.Direction.Id == direction);

        foreach (var relation in relations)
        {
            return relation.To;
        }

        return null;
    }
}

/// <summary>
/// Represents the endgame status of a chess position.
/// </summary>
public enum EndgameStatus
{
    /// <summary>
    /// The game is still in progress.
    /// </summary>
    InProgress,

    /// <summary>
    /// The active player is in check.
    /// </summary>
    Check,

    /// <summary>
    /// The active player is in checkmate.
    /// </summary>
    Checkmate,

    /// <summary>
    /// The position is stalemate.
    /// </summary>
    Stalemate
}
