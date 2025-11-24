using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Chess.MoveGeneration;
using Veggerby.Boards.Flows.Events;

namespace Veggerby.Boards.Chess;

/// <summary>
/// Chess nomenclature implementing a basic subset of Standard Algebraic Notation (SAN).
/// </summary>
/// <remarks>
/// Supports: piece letter (K,Q,R,B,N) or omitted for pawns, captures ("x") including pawn capture file notation (exd5), destination square.
/// Limitations: no disambiguation (file/rank) for same-piece targets, no check/checkmate (+/#), no castling (O-O / O-O-O), no promotion (e8=Q), and no annotations.
/// Additional context (multi-piece move generation, king attack detection, promotion events) is required for full SAN.
/// </remarks>
public sealed class ChessNomenclature : IGameNomenclature
{
    /// <inheritdoc />
    public string GetPieceName(Piece piece)
    {
        if (piece is null)
        {
            return string.Empty;
        }
        // Id pattern examples: white-pawn-5, black-knight-2
        var parts = piece.Id.Split('-');
        if (parts.Length < 3)
        {
            return piece.Id;
        }
        var color = parts[0];
        var role = parts[1];
        return role switch
        {
            "pawn" => color == Veggerby.Boards.Chess.Constants.ChessIds.Players.White ? "P" : "p",
            "rook" => color == Veggerby.Boards.Chess.Constants.ChessIds.Players.White ? "R" : "r",
            "knight" => color == Veggerby.Boards.Chess.Constants.ChessIds.Players.White ? "N" : "n",
            "bishop" => color == Veggerby.Boards.Chess.Constants.ChessIds.Players.White ? "B" : "b",
            "queen" => color == Veggerby.Boards.Chess.Constants.ChessIds.Players.White ? "Q" : "q",
            "king" => color == Veggerby.Boards.Chess.Constants.ChessIds.Players.White ? "K" : "k",
            _ => piece.Id
        };
    }

    /// <inheritdoc />
    public string GetTileName(Tile tile)
    {
        if (tile is null)
        {
            return string.Empty;
        }
        // tile ids: tile-e4 -> e4
        if (tile.Id.StartsWith("tile-"))
        {
            return tile.Id[5..];
        }
        return tile.Id;
    }

    /// <inheritdoc />
    public string GetDiceName(Dice dice) => dice?.Id ?? string.Empty; // chess typically has no dice

    /// <inheritdoc />
    public string GetPlayerName(Player player)
    {
        if (player is null)
        {
            return string.Empty;
        }
        return player.Id switch
        {
            Veggerby.Boards.Chess.Constants.ChessIds.Players.White => Veggerby.Boards.Chess.Constants.ChessIds.Players.White,
            Veggerby.Boards.Chess.Constants.ChessIds.Players.Black => Veggerby.Boards.Chess.Constants.ChessIds.Players.Black,
            _ => player.Id
        };
    }

    /// <inheritdoc />
    public string Describe(MovePieceGameEvent moveEvent)
    {
        if (moveEvent is null)
        {
            return string.Empty;
        }

        // Destination SAN square
        var toTile = moveEvent.Path?.To;
        var to = toTile is null ? string.Empty : GetTileName(toTile);
        if (string.IsNullOrEmpty(to))
        {
            return string.Empty;
        }

        // Determine piece designator (omit for pawns in SAN)
        var pieceIdParts = moveEvent.Piece.Id.Split('-');
        var role = pieceIdParts.Length > 1 ? pieceIdParts[1] : string.Empty;
        string designator = role switch
        {
            "king" => "K",
            "queen" => "Q",
            "rook" => "R",
            "bishop" => "B",
            "knight" => "N",
            _ => string.Empty // pawns -> empty
        };

        // Basic SAN: [Piece]Destination (no capture/disambiguation yet)
        return $"{designator}{to}";
    }

    /// <inheritdoc />
    public string Describe(States.GameState state, MovePieceGameEvent moveEvent)
    {
        if (moveEvent is null || moveEvent.Piece is null)
        {
            return string.Empty;
        }

        var toTile = moveEvent.Path?.To;
        var to = toTile is null ? string.Empty : GetTileName(toTile);
        if (string.IsNullOrEmpty(to))
        {
            return string.Empty;
        }

        var parts = moveEvent.Piece.Id.Split('-');
        var role = parts.Length > 1 ? parts[1] : string.Empty;
        string designator = role switch
        {
            "king" => "K",
            "queen" => "Q",
            "rook" => "R",
            "bishop" => "B",
            "knight" => "N",
            _ => string.Empty // pawns -> blank
        };

        bool isPawn = string.IsNullOrEmpty(designator);

        // Capture detection: opposing piece present on destination in pre-move state.
        bool isCapture = false;
        string pawnFile = string.Empty;
        if (state is not null && toTile is not null)
        {
            var destPieces = state.GetStates<States.PieceState>()
                .Where(ps => ps.CurrentTile == toTile)
                .Select(ps => ps.Artifact)
                .ToList();
            if (destPieces.Any())
            {
                var owner = moveEvent.Piece.Owner;
                if (owner is not null && destPieces.Any(p => !p.Owner.Equals(owner)))
                {
                    isCapture = true;
                }
            }
        }

        if (isPawn && isCapture)
        {
            // Derive file letter from origin square id (tile-e4 -> e4 -> file 'e')
            var fromTile = moveEvent.Path?.From;
            var fromSquare = fromTile is null ? string.Empty : GetTileName(fromTile);
            if (!string.IsNullOrEmpty(fromSquare))
            {
                pawnFile = fromSquare.Substring(0, 1);
            }
        }

        if (isCapture)
        {
            if (isPawn)
            {
                return $"{pawnFile}x{to}";
            }
            return $"{designator}x{to}";
        }

        // Non-capture: fallback to basic SAN
        if (isPawn)
        {
            return to;
        }
        return $"{designator}{to}";
    }

    /// <inheritdoc />
    public string Describe<TValue>(RollDiceGameEvent<TValue> rollDiceEvent)
    {
        if (rollDiceEvent is null)
        {
            return string.Empty;
        }
        return $"roll[{string.Join(',', rollDiceEvent.NewDiceStates.Select(d => d.CurrentValue))}]";
    }

    /// <inheritdoc />
    public string Describe(Game game, States.GameState state, MovePieceGameEvent moveEvent)
    {
        if (moveEvent?.Piece is null)
        {
            return string.Empty;
        }

        // Castling detection: king moves exactly two horizontal squares on same rank via two consistent east/west relations.
        if (moveEvent.Path is not null)
        {
            var idParts = moveEvent.Piece.Id.Split('-');
            if (idParts.Length > 1 && idParts[1] == "king")
            {
                var relations = moveEvent.Path.Relations.ToList();
                if (relations.Count == 2)
                {
                    var firstFrom = relations.First().From;
                    var lastTo = relations.Last().To;
                    if (firstFrom is not null && lastTo is not null)
                    {
                        var firstFromName = GetTileName(firstFrom);
                        var lastToName = GetTileName(lastTo);
                        if (firstFromName.Length == 2 && lastToName.Length == 2)
                        {
                            bool sameRank = firstFromName[1] == lastToName[1];
                            bool allEast = relations.All(r => r.Direction == Direction.East);
                            bool allWest = relations.All(r => r.Direction == Direction.West);
                            if (sameRank && (allEast || allWest))
                            {
                                var notation = allEast ? "O-O" : "O-O-O"; // east assumed king-side, west queen-side
                                if (IsCheckAfter(game, state, moveEvent))
                                {
                                    notation += "+";
                                }
                                if (IsCheckmateAfter(game, state, moveEvent))
                                {
                                    notation += "#";
                                }
                                return notation;
                            }
                        }
                    }
                }
            }
        }

        // Base SAN / capture from state-aware method
        var baseText = Describe(state, moveEvent);
        if (string.IsNullOrEmpty(baseText))
        {
            return string.Empty;
        }

        var pieceParts = moveEvent.Piece.Id.Split('-');
        if (pieceParts.Length < 2)
        {
            return baseText; // cannot infer role
        }
        var role = pieceParts[1];
        bool isPawn = role == "pawn";

        // Promotion detection (simplified): pawn reaches last rank (white: rank 8, black: rank 1)
        if (isPawn && moveEvent.Path?.To is not null)
        {
            var toSquare = GetTileName(moveEvent.Path.To);
            if (toSquare.Length == 2)
            {
                var rankChar = toSquare[1];
                var ownerId = moveEvent.Piece.Owner?.Id;
                bool promote = (ownerId == Veggerby.Boards.Chess.Constants.ChessIds.Players.White && rankChar == '8') || (ownerId == Veggerby.Boards.Chess.Constants.ChessIds.Players.Black && rankChar == '1');
                if (promote)
                {
                    var promo = baseText + "=Q"; // assume queen promotion
                    if (IsCheckAfter(game, state, moveEvent))
                    {
                        promo += "+";
                    }
                    if (IsCheckmateAfter(game, state, moveEvent))
                    {
                        promo += "#";
                    }
                    return promo;
                }
            }
            return baseText; // normal pawn move already correct
        }

        if (isPawn)
        {
            return baseText; // pawn notation complete
        }

        // Recompute destination and designator for disambiguation
        var destTile = moveEvent.Path?.To;
        if (destTile is null)
        {
            return baseText;
        }
        var to = GetTileName(destTile);
        if (string.IsNullOrEmpty(to))
        {
            return baseText;
        }
        var designator = role switch
        {
            "king" => "K",
            "queen" => "Q",
            "rook" => "R",
            "bishop" => "B",
            "knight" => "N",
            _ => string.Empty
        };
        bool isCapture = baseText.Contains('x');

        var moverOwner = moveEvent.Piece.Owner;
        if (state is null || moverOwner is null)
        {
            return baseText;
        }

        var candidateStates = state.GetStates<States.PieceState>()
            .Where(ps => ps.Artifact != moveEvent.Piece && ps.Artifact.Owner == moverOwner && ps.Artifact.Id.Split('-').Length > 1 && ps.Artifact.Id.Split('-')[1] == role)
            .ToList();
        if (!candidateStates.Any())
        {
            // No ambiguity; append check/mate markers
            var suffix = string.Empty;
            if (IsCheckAfter(game, state, moveEvent))
            {
                suffix += "+";
            }
            if (IsCheckmateAfter(game, state, moveEvent))
            {
                suffix += "#";
            }
            return baseText + suffix;
        }

        var fromTile = moveEvent.Path?.From;
        if (fromTile is null)
        {
            return baseText; // cannot disambiguate
        }
        var fromSquare = GetTileName(fromTile);
        if (fromSquare.Length < 2)
        {
            return baseText;
        }
        var moverFile = fromSquare[0].ToString();
        var moverRank = fromSquare[1].ToString();

        bool AnyCanReach(States.PieceState ps)
        {
            foreach (var pattern in ps.Artifact.Patterns)
            {
                var visitor = new ResolveTilePathPatternVisitor(game.Board, ps.CurrentTile, destTile);
                pattern.Accept(visitor);
                if (visitor.ResultPath is not null)
                {
                    return true;
                }
            }
            return false;
        }

        var ambiguous = candidateStates.Where(AnyCanReach).ToList();
        if (!ambiguous.Any())
        {
            return baseText; // no alternative pieces
        }

        var allFiles = ambiguous.Select(ps => GetTileName(ps.CurrentTile)[0]).Concat(new[] { moverFile[0] }).Distinct().ToList();
        var allRanks = ambiguous.Select(ps => GetTileName(ps.CurrentTile)[1]).Concat(new[] { moverRank[0] }).Distinct().ToList();

        string disambiguator = string.Empty;
        if (allFiles.Count > 1)
        {
            disambiguator = moverFile;
        }
        else if (allRanks.Count > 1)
        {
            disambiguator = moverRank;
        }
        else
        {
            disambiguator = moverFile + moverRank;
        }

        string result = isCapture ? $"{designator}{disambiguator}x{to}" : $"{designator}{disambiguator}{to}";
        if (IsCheckAfter(game, state, moveEvent))
        {
            result += "+";
        }
        if (IsCheckmateAfter(game, state, moveEvent))
        {
            result += "#";
        }
        return result;
    }

    private bool IsCheckAfter(Game game, States.GameState state, MovePieceGameEvent moveEvent)
    {
        if (game is null || state is null || moveEvent?.Piece?.Owner is null || moveEvent.Path?.To is null)
        {
            return false;
        }

        // Find opposing king
        var moverOwner = moveEvent.Piece.Owner;
        var opponentKingState = state.GetStates<States.PieceState>()
            .FirstOrDefault(ps => ps.Artifact.Owner != moverOwner && (ChessPiece.IsKing(game, ps.Artifact.Id) || ps.Artifact.Id.Contains("-king")));

        if (opponentKingState is null)
        {
            return false; // cannot detect
        }

        // Build hypothetical post-move state (simplified): update moving piece position, remove any captured victim on destination
        var destination = moveEvent.Path?.To;
        if (destination is null)
        {
            return false;
        }
        var updatedStates = state.GetStates<States.PieceState>()
            .Where(ps => ps.Artifact != moveEvent.Piece && ps.CurrentTile != destination) // remove captured piece if any opposing there
            .Select(ps => (States.IArtifactState)ps)
            .ToList();

        updatedStates.Add(new States.PieceState(moveEvent.Piece, destination));
        var post = States.GameState.New(updatedStates);

        // For each mover side piece, see if any pattern reaches king square
        var kingTile = opponentKingState.CurrentTile;
        foreach (var ps in post.GetStates<States.PieceState>().Where(x => x.Artifact.Owner == moverOwner))
        {
            foreach (var pattern in ps.Artifact.Patterns)
            {
                var visitor = new ResolveTilePathPatternVisitor(game.Board, ps.CurrentTile, kingTile);
                pattern.Accept(visitor);
                if (visitor.ResultPath is not null)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private bool IsCheckmateAfter(Game game, States.GameState state, MovePieceGameEvent moveEvent)
    {
        if (game is null || state is null || moveEvent?.Piece?.Owner is null || moveEvent.Path?.To is null)
        {
            return false;
        }

        try
        {
            // Simulate the move to get the resulting state
            var destination = moveEvent.Path.To;
            var movedPiece = moveEvent.Piece;

            // Create a simplified post-move state
            var updatedStates = state.GetStates<States.PieceState>()
                .Where(ps => ps.Artifact != movedPiece && ps.CurrentTile != destination)
                .Select(ps => (States.IArtifactState)ps)
                .ToList();

            updatedStates.Add(new States.PieceState(movedPiece, destination));

            // Switch active player for the resulting state
            var currentActivePlayer = movedPiece.Owner;
            var players = game.Players.ToList();
            var opponentPlayer = players.FirstOrDefault(p => p.Id != currentActivePlayer?.Id);

            if (opponentPlayer != null)
            {
                // Update active player states
                foreach (var player in players)
                {
                    updatedStates.Add(new States.ActivePlayerState(player, player.Id == opponentPlayer.Id));
                }
            }

            var postState = States.GameState.New(updatedStates);

            // Use the endgame detector to check for checkmate
            var detector = new ChessEndgameDetector(game);
            return detector.IsCheckmate(postState);
        }
        catch
        {
            return false;
        }
    }
}