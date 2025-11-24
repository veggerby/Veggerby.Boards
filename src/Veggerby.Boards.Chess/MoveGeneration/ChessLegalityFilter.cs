using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Chess.MoveGeneration;

/// <summary>
/// Filters pseudo-legal moves to remove those that would leave the player's king in check.
/// </summary>
/// <remarks>
/// This class takes pseudo-legal moves and validates that they don't result in self-check.
/// A move is legal only if after making it, the player's own king is not under attack.
/// </remarks>
public sealed class ChessLegalityFilter
{
    private readonly Game _game;
    private readonly ChessMoveGenerator _moveGenerator;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChessLegalityFilter"/> class.
    /// </summary>
    /// <param name="game">The chess game definition.</param>
    public ChessLegalityFilter(Game game)
    {
        _game = game ?? throw new ArgumentNullException(nameof(game));
        _moveGenerator = new ChessMoveGenerator(game);
    }

    /// <summary>
    /// Filters pseudo-legal moves to return only legal moves.
    /// </summary>
    /// <param name="state">Current game state.</param>
    /// <param name="pseudoLegalMoves">Collection of pseudo-legal moves to filter.</param>
    /// <returns>Collection of legal moves (moves that don't leave own king in check).</returns>
    public IReadOnlyCollection<PseudoMove> FilterLegalMoves(GameState state, IReadOnlyCollection<PseudoMove> pseudoLegalMoves)
    {
        if (state == null)
            throw new ArgumentNullException(nameof(state));
        if (pseudoLegalMoves == null)
            throw new ArgumentNullException(nameof(pseudoLegalMoves));

        var legalMoves = new List<PseudoMove>();

        if (!state.TryGetActivePlayer(out var activePlayer) || activePlayer == null)
        {
            return legalMoves;
        }

        foreach (var move in pseudoLegalMoves)
        {
            if (IsMoveLegal(state, move, activePlayer))
            {
                legalMoves.Add(move);
            }
        }

        return legalMoves;
    }

    /// <summary>
    /// Generates all legal moves for the active player.
    /// </summary>
    /// <param name="state">Current game state.</param>
    /// <returns>Collection of legal moves.</returns>
    public IReadOnlyCollection<PseudoMove> GenerateLegalMoves(GameState state)
    {
        var pseudoLegalMoves = _moveGenerator.Generate(state);
        return FilterLegalMoves(state, pseudoLegalMoves);
    }

    private bool IsMoveLegal(GameState state, PseudoMove move, Player activePlayer)
    {
        // Simulate the move and check if king is in check
        var simulatedState = SimulateMove(state, move);

        if (simulatedState == null)
        {
            return false;
        }

        // Check if the king is under attack in the simulated state
        return !IsKingInCheck(simulatedState, activePlayer);
    }

    private GameState? SimulateMove(GameState state, PseudoMove move)
    {
        try
        {
            // Create a new state with the piece moved
            var pieceState = state.GetState<PieceState>(move.Piece);

            if (pieceState == null)
            {
                return null;
            }

            // Remove the piece from its current position and place it at the new position
            var newPieceState = new PieceState(move.Piece, move.To);
            var newState = state.Next([newPieceState]);

            // Handle captures - if there's a piece at the destination, mark it as captured
            if (move.IsCapture)
            {
                var capturedPieces = state.GetPiecesOnTile(move.To).Where(p => p.Owner?.Id != move.Piece.Owner?.Id);

                foreach (var capturedPiece in capturedPieces)
                {
                    var capturedState = new CapturedPieceState(capturedPiece);
                    newState = newState.Next([capturedState]);
                }
            }

            // Handle en passant captures
            if (move.Kind == PseudoMoveKind.EnPassant)
            {
                // The captured pawn is on the same rank as the moving pawn, not on the destination square
                var isWhite = ChessPiece.IsWhite(_game, move.Piece.Id);
                var captureDirection = isWhite ? Veggerby.Boards.Constants.Directions.South : Veggerby.Boards.Constants.Directions.North;
                var capturedPawnTile = GetRelatedTile(move.To, captureDirection);

                if (capturedPawnTile != null)
                {
                    var capturedPawns = state.GetPiecesOnTile(capturedPawnTile).Where(p => p.Owner?.Id != move.Piece.Owner?.Id);

                    foreach (var capturedPawn in capturedPawns)
                    {
                        var capturedState = new CapturedPieceState(capturedPawn);
                        newState = newState.Next([capturedState]);
                    }
                }
            }

            // Handle castling - move the rook as well
            if (move.Kind == PseudoMoveKind.Castle)
            {
                var isKingSide = IsEastOf(move.To, move.From);
                var rookOrigin = GetCastleRookOrigin(move.From, isKingSide);
                var rookTarget = GetRelatedTile(move.To, isKingSide ? Veggerby.Boards.Constants.Directions.West : Veggerby.Boards.Constants.Directions.East);

                if (rookOrigin != null && rookTarget != null)
                {
                    var rooks = state.GetPiecesOnTile(rookOrigin).Where(p => ChessPiece.IsRook(_game, p.Id));

                    foreach (var rook in rooks)
                    {
                        var rookState = new PieceState(rook, rookTarget);
                        newState = newState.Next([rookState]);
                    }
                }
            }

            return newState;
        }
        catch
        {
            return null;
        }
    }

    private bool IsKingInCheck(GameState state, Player player)
    {
        // Find the king's position
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

        var kingTile = kingState.CurrentTile;

        // Check if any opponent piece can attack the king's position
        return IsSquareUnderAttack(state, kingTile, player);
    }

    private Piece? FindKing(GameState state, Player player)
    {
        var pieceStates = state.GetStates<PieceState>();

        foreach (var pieceState in pieceStates)
        {
            if (pieceState.Artifact.Owner?.Id == player.Id &&
                ChessPiece.IsKing(_game, pieceState.Artifact.Id) &&
                !state.IsCaptured(pieceState.Artifact))
            {
                return pieceState.Artifact;
            }
        }

        return null;
    }

    private bool IsSquareUnderAttack(GameState state, Tile square, Player playerToProtect)
    {
        // Check all opponent pieces to see if they can attack this square
        var pieceStates = state.GetStates<PieceState>();

        foreach (var pieceState in pieceStates)
        {
            if (state.IsCaptured(pieceState.Artifact))
            {
                continue;
            }

            if (pieceState.Artifact.Owner?.Id == playerToProtect.Id)
            {
                continue;
            }

            // Check if this opponent piece can attack the square
            if (CanPieceAttackSquare(state, pieceState, square))
            {
                return true;
            }
        }

        return false;
    }

    private bool CanPieceAttackSquare(GameState state, PieceState pieceState, Tile targetSquare)
    {
        var piece = pieceState.Artifact;
        var from = pieceState.CurrentTile;

        if (from == null)
        {
            return false;
        }

        if (ChessPiece.IsPawn(_game, piece.Id))
        {
            return CanPawnAttackSquare(state, piece, from, targetSquare);
        }
        else if (ChessPiece.IsKnight(_game, piece.Id))
        {
            return CanKnightAttackSquare(from, targetSquare);
        }
        else if (ChessPiece.IsBishop(_game, piece.Id))
        {
            return CanSlidingPieceAttackSquare(state, from, targetSquare, diagonal: true, orthogonal: false);
        }
        else if (ChessPiece.IsRook(_game, piece.Id))
        {
            return CanSlidingPieceAttackSquare(state, from, targetSquare, diagonal: false, orthogonal: true);
        }
        else if (ChessPiece.IsQueen(_game, piece.Id))
        {
            return CanSlidingPieceAttackSquare(state, from, targetSquare, diagonal: true, orthogonal: true);
        }
        else if (ChessPiece.IsKing(_game, piece.Id))
        {
            return CanKingAttackSquare(from, targetSquare);
        }

        return false;
    }

    private bool CanPawnAttackSquare(GameState state, Piece pawn, Tile from, Tile targetSquare)
    {
        var isWhite = ChessPiece.IsWhite(_game, pawn.Id);
        var diagonalDirections = isWhite
            ? new[] { Veggerby.Boards.Constants.Directions.NorthEast, Veggerby.Boards.Constants.Directions.NorthWest }
            : new[] { Veggerby.Boards.Constants.Directions.SouthEast, Veggerby.Boards.Constants.Directions.SouthWest };

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

    private bool CanKnightAttackSquare(Tile from, Tile targetSquare)
    {
        var knightOffsets = new[]
        {
            (Veggerby.Boards.Constants.Directions.North, Veggerby.Boards.Constants.Directions.North, Veggerby.Boards.Constants.Directions.East),
            (Veggerby.Boards.Constants.Directions.North, Veggerby.Boards.Constants.Directions.North, Veggerby.Boards.Constants.Directions.West),
            (Veggerby.Boards.Constants.Directions.South, Veggerby.Boards.Constants.Directions.South, Veggerby.Boards.Constants.Directions.East),
            (Veggerby.Boards.Constants.Directions.South, Veggerby.Boards.Constants.Directions.South, Veggerby.Boards.Constants.Directions.West),
            (Veggerby.Boards.Constants.Directions.East, Veggerby.Boards.Constants.Directions.East, Veggerby.Boards.Constants.Directions.North),
            (Veggerby.Boards.Constants.Directions.East, Veggerby.Boards.Constants.Directions.East, Veggerby.Boards.Constants.Directions.South),
            (Veggerby.Boards.Constants.Directions.West, Veggerby.Boards.Constants.Directions.West, Veggerby.Boards.Constants.Directions.North),
            (Veggerby.Boards.Constants.Directions.West, Veggerby.Boards.Constants.Directions.West, Veggerby.Boards.Constants.Directions.South)
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

    private bool CanSlidingPieceAttackSquare(GameState state, Tile from, Tile targetSquare, bool diagonal, bool orthogonal)
    {
        var directions = new List<string>();

        if (diagonal)
        {
            directions.Add(Veggerby.Boards.Constants.Directions.NorthEast);
            directions.Add(Veggerby.Boards.Constants.Directions.NorthWest);
            directions.Add(Veggerby.Boards.Constants.Directions.SouthEast);
            directions.Add(Veggerby.Boards.Constants.Directions.SouthWest);
        }

        if (orthogonal)
        {
            directions.Add(Veggerby.Boards.Constants.Directions.North);
            directions.Add(Veggerby.Boards.Constants.Directions.South);
            directions.Add(Veggerby.Boards.Constants.Directions.East);
            directions.Add(Veggerby.Boards.Constants.Directions.West);
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

                // Check if the square is occupied (blocking the attack)
                if (IsOccupied(state, next))
                {
                    break;
                }

                current = next;
            }
        }

        return false;
    }

    private bool CanKingAttackSquare(Tile from, Tile targetSquare)
    {
        var directions = new[]
        {
            Veggerby.Boards.Constants.Directions.North,
            Veggerby.Boards.Constants.Directions.South,
            Veggerby.Boards.Constants.Directions.East,
            Veggerby.Boards.Constants.Directions.West,
            Veggerby.Boards.Constants.Directions.NorthEast,
            Veggerby.Boards.Constants.Directions.NorthWest,
            Veggerby.Boards.Constants.Directions.SouthEast,
            Veggerby.Boards.Constants.Directions.SouthWest
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

    private bool IsOccupied(GameState state, Tile tile)
    {
        return state.GetPiecesOnTile(tile).Any();
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

    private Tile? GetCastleRookOrigin(Tile from, bool kingSide)
    {
        var direction = kingSide ? Veggerby.Boards.Constants.Directions.East : Veggerby.Boards.Constants.Directions.West;
        var current = from;

        while (true)
        {
            var next = GetRelatedTile(current, direction);

            if (next == null)
            {
                return current;
            }

            current = next;
        }
    }

    private bool IsEastOf(Tile target, Tile origin)
    {
        var current = origin;

        while (true)
        {
            var next = GetRelatedTile(current, Veggerby.Boards.Constants.Directions.East);

            if (next == null)
            {
                return false;
            }

            if (next == target)
            {
                return true;
            }

            current = next;
        }
    }
}
