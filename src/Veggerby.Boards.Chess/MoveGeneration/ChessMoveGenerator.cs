using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Chess.MoveGeneration;

/// <summary>
/// Generates pseudo-legal moves for chess pieces without regard to king safety.
/// </summary>
/// <remarks>
/// Pseudo-legal moves include all geometrically valid moves for a piece given the current board state
/// (respecting occupancy and capture rules) but may leave the player's own king in check.
/// A separate legality filter must be applied to exclude moves that result in self-check.
/// </remarks>
public sealed class ChessMoveGenerator
{
    private readonly Game _game;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChessMoveGenerator"/> class.
    /// </summary>
    /// <param name="game">The chess game definition.</param>
    /// <exception cref="ArgumentNullException">Thrown if game is null.</exception>
    public ChessMoveGenerator(Game game)
    {
        _game = game ?? throw new ArgumentNullException(nameof(game));
    }

    /// <summary>
    /// Generates all pseudo-legal moves for the active player in the given state.
    /// </summary>
    /// <param name="state">Current game state.</param>
    /// <returns>Collection of pseudo-legal moves.</returns>
    /// <exception cref="ArgumentNullException">Thrown if state is null.</exception>
    /// <remarks>
    /// Performance consideration: uses explicit loops over LINQ for allocation efficiency.
    /// </remarks>
    public IReadOnlyCollection<PseudoMove> Generate(GameState state)
    {
        if (state is null)
        {
            throw new ArgumentNullException(nameof(state));
        }

        var moves = new List<PseudoMove>();

        if (!state.TryGetActivePlayer(out var activePlayer) || activePlayer is null)
        {
            return moves;
        }

        var pieceStates = state.GetStates<PieceState>();

        foreach (var pieceState in pieceStates)
        {
            if (pieceState.Artifact.Owner?.Id != activePlayer.Id)
            {
                continue;
            }

            if (state.IsCaptured(pieceState.Artifact))
            {
                continue;
            }

            GenerateMovesForPiece(state, pieceState, moves);
        }

        return moves;
    }

    private void GenerateMovesForPiece(GameState state, PieceState pieceState, List<PseudoMove> moves)
    {
        var piece = pieceState.Artifact;
        var from = pieceState.CurrentTile;

        if (from is null)
        {
            return;
        }

        if (ChessPiece.IsPawn(state, piece.Id))
        {
            GeneratePawnMoves(state, pieceState, moves);
        }
        else if (ChessPiece.IsKnight(state, piece.Id))
        {
            GenerateKnightMoves(state, pieceState, moves);
        }
        else if (ChessPiece.IsBishop(state, piece.Id))
        {
            GenerateSlidingMoves(state, pieceState, moves, diagonal: true, orthogonal: false);
        }
        else if (ChessPiece.IsRook(state, piece.Id))
        {
            GenerateSlidingMoves(state, pieceState, moves, diagonal: false, orthogonal: true);
        }
        else if (ChessPiece.IsQueen(state, piece.Id))
        {
            GenerateSlidingMoves(state, pieceState, moves, diagonal: true, orthogonal: true);
        }
        else if (ChessPiece.IsKing(state, piece.Id))
        {
            GenerateKingMoves(state, pieceState, moves);
        }
    }

    private void GeneratePawnMoves(GameState state, PieceState pieceState, List<PseudoMove> moves)
    {
        var piece = pieceState.Artifact;
        var from = pieceState.CurrentTile;

        if (from is null)
        {
            return;
        }

        var isWhite = ChessPiece.IsWhite(state, piece.Id);
        var forwardDirection = isWhite ? Constants.Directions.North : Constants.Directions.South;
        var forward = GetRelatedTile(from, forwardDirection);

        if (forward is not null && !IsOccupied(state, forward))
        {
            if (IsPromotionRank(forward, isWhite))
            {
                AddPromotionMoves(piece, from, forward, isCapture: false, moves);
            }
            else
            {
                moves.Add(new PseudoMove(piece, from, forward, PseudoMoveKind.Normal, null, false));

                if (IsStartingRank(from, isWhite))
                {
                    var doubleForward = GetRelatedTile(forward, forwardDirection);

                    if (doubleForward is not null && !IsOccupied(state, doubleForward))
                    {
                        moves.Add(new PseudoMove(piece, from, doubleForward, PseudoMoveKind.Normal, null, false));
                    }
                }
            }
        }

        var diagonalDirections = isWhite
            ? new[] { Constants.Directions.NorthEast, Constants.Directions.NorthWest }
            : new[] { Constants.Directions.SouthEast, Constants.Directions.SouthWest };

        foreach (var diagDir in diagonalDirections)
        {
            var diagonal = GetRelatedTile(from, diagDir);

            if (diagonal is null)
            {
                continue;
            }

            if (IsOccupiedByOpponent(state, diagonal, piece.Owner))
            {
                if (IsPromotionRank(diagonal, isWhite))
                {
                    AddPromotionMoves(piece, from, diagonal, isCapture: true, moves);
                }
                else
                {
                    moves.Add(new PseudoMove(piece, from, diagonal, PseudoMoveKind.Normal, null, true));
                }
            }
            else if (IsEnPassantTarget(state, diagonal))
            {
                moves.Add(new PseudoMove(piece, from, diagonal, PseudoMoveKind.EnPassant, null, true));
            }
        }
    }

    private void GenerateKnightMoves(GameState state, PieceState pieceState, List<PseudoMove> moves)
    {
        var piece = pieceState.Artifact;
        var from = pieceState.CurrentTile;

        if (from is null)
        {
            return;
        }

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

            if (intermediate1 is null)
            {
                continue;
            }

            var intermediate2 = GetRelatedTile(intermediate1, second);

            if (intermediate2 is null)
            {
                continue;
            }

            var target = GetRelatedTile(intermediate2, third);

            if (target is null)
            {
                continue;
            }

            if (!IsOccupiedByFriendly(state, target, piece.Owner))
            {
                var isCapture = IsOccupiedByOpponent(state, target, piece.Owner);
                moves.Add(new PseudoMove(piece, from, target, PseudoMoveKind.Normal, null, isCapture));
            }
        }
    }

    private void GenerateSlidingMoves(GameState state, PieceState pieceState, List<PseudoMove> moves, bool diagonal, bool orthogonal)
    {
        var piece = pieceState.Artifact;
        var from = pieceState.CurrentTile;

        if (from is null)
        {
            return;
        }

        var directions = new List<string>();

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

                if (next is null)
                {
                    break;
                }

                if (IsOccupiedByFriendly(state, next, piece.Owner))
                {
                    break;
                }

                var isCapture = IsOccupiedByOpponent(state, next, piece.Owner);
                moves.Add(new PseudoMove(piece, from, next, PseudoMoveKind.Normal, null, isCapture));

                if (isCapture)
                {
                    break;
                }

                current = next;
            }
        }
    }

    private void GenerateKingMoves(GameState state, PieceState pieceState, List<PseudoMove> moves)
    {
        var piece = pieceState.Artifact;
        var from = pieceState.CurrentTile;

        if (from is null)
        {
            return;
        }

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

            if (target is null)
            {
                continue;
            }

            if (!IsOccupiedByFriendly(state, target, piece.Owner))
            {
                var isCapture = IsOccupiedByOpponent(state, target, piece.Owner);
                moves.Add(new PseudoMove(piece, from, target, PseudoMoveKind.Normal, null, isCapture));
            }
        }

        GenerateCastlingMoves(state, pieceState, moves);
    }

    private void GenerateCastlingMoves(GameState state, PieceState pieceState, List<PseudoMove> moves)
    {
        var piece = pieceState.Artifact;
        var from = pieceState.CurrentTile;

        if (from is null)
        {
            return;
        }

        var isWhite = ChessPiece.IsWhite(state, piece.Id);
        var extras = state.GetExtras<ChessStateExtras>();

        if (extras is null)
        {
            return;
        }

        if ((isWhite && extras.WhiteCanCastleKingSide) || (!isWhite && extras.BlackCanCastleKingSide))
        {
            var kingTarget = GetCastleKingTarget(from, kingSide: true);
            var rookOrigin = GetCastleRookOrigin(from, kingSide: true);

            if (kingTarget is not null && rookOrigin is not null && IsCastlingPathClear(state, from, kingTarget, rookOrigin))
            {
                moves.Add(new PseudoMove(piece, from, kingTarget, PseudoMoveKind.Castle, null, false));
            }
        }

        if ((isWhite && extras.WhiteCanCastleQueenSide) || (!isWhite && extras.BlackCanCastleQueenSide))
        {
            var kingTarget = GetCastleKingTarget(from, kingSide: false);
            var rookOrigin = GetCastleRookOrigin(from, kingSide: false);

            if (kingTarget is not null && rookOrigin is not null && IsCastlingPathClear(state, from, kingTarget, rookOrigin))
            {
                moves.Add(new PseudoMove(piece, from, kingTarget, PseudoMoveKind.Castle, null, false));
            }
        }
    }

    private Tile? GetCastleKingTarget(Tile from, bool kingSide)
    {
        var direction = kingSide ? Constants.Directions.East : Constants.Directions.West;
        var first = GetRelatedTile(from, direction);

        if (first is null)
        {
            return null;
        }

        return GetRelatedTile(first, direction);
    }

    private Tile? GetCastleRookOrigin(Tile from, bool kingSide)
    {
        var direction = kingSide ? Constants.Directions.East : Constants.Directions.West;
        var current = from;

        while (true)
        {
            var next = GetRelatedTile(current, direction);

            if (next is null)
            {
                return current;
            }

            current = next;
        }
    }

    private bool IsCastlingPathClear(GameState state, Tile kingOrigin, Tile kingTarget, Tile rookOrigin)
    {
        var kingSide = IsEastOf(kingTarget, kingOrigin);
        var direction = kingSide ? Constants.Directions.East : Constants.Directions.West;
        var current = kingOrigin;

        while (current != kingTarget)
        {
            var next = GetRelatedTile(current, direction);

            if (next is null)
            {
                return false;
            }

            if (next != kingTarget && IsOccupied(state, next))
            {
                return false;
            }

            current = next;
        }

        var rookTarget = GetRelatedTile(kingTarget, kingSide ? Constants.Directions.West : Constants.Directions.East);

        if (rookTarget is null)
        {
            return false;
        }

        current = rookOrigin;

        while (current != rookTarget)
        {
            var next = GetRelatedTile(current, kingSide ? Constants.Directions.West : Constants.Directions.East);

            if (next is null)
            {
                return false;
            }

            if (next != kingOrigin && next != rookTarget && IsOccupied(state, next))
            {
                return false;
            }

            current = next;
        }

        return true;
    }

    private bool IsEastOf(Tile target, Tile origin)
    {
        var current = origin;

        while (true)
        {
            var next = GetRelatedTile(current, Constants.Directions.East);

            if (next is null)
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

    private void AddPromotionMoves(Piece piece, Tile from, Tile to, bool isCapture, List<PseudoMove> moves)
    {
        var promotionRoles = new[] { "queen", "rook", "bishop", "knight" };

        foreach (var role in promotionRoles)
        {
            moves.Add(new PseudoMove(piece, from, to, PseudoMoveKind.Promotion, role, isCapture));
        }
    }

    private bool IsStartingRank(Tile tile, bool isWhite)
    {
        var tileName = tile.Id;

        if (!tileName.StartsWith("tile-"))
        {
            return false;
        }

        var coords = tileName[5..];

        if (coords.Length < 2)
        {
            return false;
        }

        var rank = coords[1];
        return isWhite ? rank == '2' : rank == '7';
    }

    private bool IsPromotionRank(Tile tile, bool isWhite)
    {
        var tileName = tile.Id;

        if (!tileName.StartsWith("tile-"))
        {
            return false;
        }

        var coords = tileName[5..];

        if (coords.Length < 2)
        {
            return false;
        }

        var rank = coords[1];
        return isWhite ? rank == '8' : rank == '1';
    }

    private bool IsEnPassantTarget(GameState state, Tile tile)
    {
        var extras = state.GetExtras<ChessStateExtras>();
        return extras is not null && extras.EnPassantTargetTileId == tile.Id;
    }

    private bool IsOccupied(GameState state, Tile tile)
    {
        return state.GetPiecesOnTile(tile).Any();
    }

    private bool IsOccupiedByFriendly(GameState state, Tile tile, Player player)
    {
        return state.GetPiecesOnTile(tile).Any(p => p.Owner?.Id == player.Id);
    }

    private bool IsOccupiedByOpponent(GameState state, Tile tile, Player player)
    {
        var pieces = state.GetPiecesOnTile(tile);
        return pieces.Any(p => p.Owner?.Id != player.Id);
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
