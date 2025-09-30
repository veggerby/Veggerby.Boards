using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Relations;
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
            "pawn" => color == ChessIds.Players.White ? "P" : "p",
            "rook" => color == ChessIds.Players.White ? "R" : "r",
            "knight" => color == ChessIds.Players.White ? "N" : "n",
            "bishop" => color == ChessIds.Players.White ? "B" : "b",
            "queen" => color == ChessIds.Players.White ? "Q" : "q",
            "king" => color == ChessIds.Players.White ? "K" : "k",
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
            ChessIds.Players.White => ChessIds.Players.White,
            ChessIds.Players.Black => ChessIds.Players.Black,
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
        var to = GetTileName(moveEvent.Path?.To);
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
        if (moveEvent is null)
        {
            return string.Empty;
        }

        var toTile = moveEvent.Path?.To;
        var to = GetTileName(toTile);
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
            var fromSquare = GetTileName(moveEvent.Path?.From);
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
        // Castling detection: king moves exactly two horizontal squares on same rank via two consistent east/west relations.
        if (moveEvent?.Piece is not null)
        {
            var idParts = moveEvent.Piece.Id.Split('-');
            if (idParts.Length > 1 && idParts[1] == "king" && moveEvent.Path is not null)
            {
                var relations = moveEvent.Path.Relations.ToList();
                if (relations.Count == 2)
                {
                    bool sameRank = GetTileName(relations.First().From)[1] == GetTileName(relations.Last().To)[1];
                    bool allEast = relations.All(r => r.Direction == Direction.East);
                    bool allWest = relations.All(r => r.Direction == Direction.West);
                    if (sameRank && (allEast || allWest))
                    {
                        var notation = allEast ? "O-O" : "O-O-O"; // east assumed king-side, west queen-side
                        // (Optional) append check marker if king move itself gives check; rook relocation ignored for now.
                        if (IsCheckAfter(game, state, moveEvent)) { notation += "+"; }
                        if (IsCheckmateAfter(game, state, moveEvent)) { notation += "#"; }
                        return notation;
                    }
                }
            }
        }
        // Start from state-aware capture implementation first
        var baseText = Describe(state, moveEvent);

        // Disambiguation not needed for pawns or when result already contains 'x' with pawn file (exd5) which is inherently disambiguated for pawns
        var parts = moveEvent.Piece.Id.Split('-');
        if (parts.Length < 2)
        {
            return baseText;
        }
        var role = parts[1];
        bool isPawn = role == "pawn";

        // Promotion detection (simplified): pawn reaches last rank (white: rank 8, black: rank 1)
        if (isPawn && moveEvent.Path?.To is not null)
        {
            var toSquare = GetTileName(moveEvent.Path.To);
            if (toSquare.Length == 2)
            {
                var rankChar = toSquare[1];
                // Assume white moves toward higher rank numbers (8), black toward lower (1)
                var ownerId = moveEvent.Piece.Owner?.Id;
                bool promote = (ownerId == ChessIds.Players.White && rankChar == '8') || (ownerId == ChessIds.Players.Black && rankChar == '1');
                if (promote)
                {
                    // baseText for pawn non-capture move is destination; capture form is exd8. Append =Q (fixed queen for now).
                    var promo = baseText + "=Q";
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
            return baseText; // no further processing (disambiguation not needed)
        }

        // Extract already computed designator and destination from baseText
        // baseText patterns: Piece[optional 'x']square or Piece square or Piecex square (no spaces actually)
        // We'll recompose instead of parsing; compute capture flag again
        var to = GetTileName(moveEvent.Path?.To);
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

        // Find other candidate pieces of same role & owner that could also move to destination
        var moverOwner = moveEvent.Piece.Owner;
        var destination = moveEvent.Path?.To;
        if (state is null || destination is null)
        {
            return baseText;
        }

        var moverFromName = GetTileName(moveEvent.Path.From);
        var moverFile = moverFromName.Length > 0 ? moverFromName[0].ToString() : string.Empty;
        var moverRank = moverFromName.Length > 1 ? moverFromName[1].ToString() : string.Empty;

        var candidateStates = state.GetStates<States.PieceState>()
            .Where(ps => ps.Artifact != moveEvent.Piece && ps.Artifact.Owner == moverOwner && ps.Artifact.Id.Split('-').Length > 1 && ps.Artifact.Id.Split('-')[1] == role)
            .ToList();

        if (!candidateStates.Any())
        {
            // No ambiguity; still append check marker if applicable
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

        bool AnyCanReach(States.PieceState ps)
        {
            foreach (var pattern in ps.Artifact.Patterns)
            {
                var visitor = new ResolveTilePathPatternVisitor(game.Board, ps.CurrentTile, destination);
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
            return baseText; // no other can reach
        }

        // Determine disambiguation by SAN rules: prefer file if differs among candidates (including mover), else rank, else both
        var moverFromTile = moveEvent.Path.From;
        string moverSquare = GetTileName(moverFromTile);

        var allFiles = ambiguous.Select(ps => GetTileName(ps.CurrentTile)[0]).Concat(new[] { moverSquare[0] }).Distinct().ToList();
        var allRanks = ambiguous.Select(ps => GetTileName(ps.CurrentTile)[1]).Concat(new[] { moverSquare[1] }).Distinct().ToList();

        string disambiguator = string.Empty;
        if (allFiles.Count > 1)
        {
            disambiguator = moverFile; // file differentiates
        }
        else if (allRanks.Count > 1)
        {
            disambiguator = moverRank; // ranks differentiate
        }
        else
        {
            disambiguator = moverFile + moverRank; // need full square (rare)
        }

        if (isCapture)
        {
            var notation = $"{designator}{disambiguator}x{to}";
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
        var simple = $"{designator}{disambiguator}{to}";
        if (IsCheckAfter(game, state, moveEvent))
        {
            simple += "+";
        }

        if (IsCheckmateAfter(game, state, moveEvent))
        {
            simple += "#";
        }

        return simple;
    }

    private bool IsCheckAfter(Game game, States.GameState state, MovePieceGameEvent moveEvent)
    {
        if (game is null || state is null || moveEvent?.Piece?.Owner is null)
        {
            return false;
        }

        // Find opposing king
        var moverOwner = moveEvent.Piece.Owner;
        var opponentKingState = state.GetStates<States.PieceState>()
            .FirstOrDefault(ps => ps.Artifact.Owner != moverOwner && ps.Artifact.Id.Contains(ChessIds.PieceSuffixes.King));
        if (opponentKingState is null)
        {
            return false; // cannot detect
        }

        // Build hypothetical post-move state (simplified): update moving piece position, remove any captured victim on destination
        var destination = moveEvent.Path.To;
        var updatedStates = state.GetStates<States.PieceState>()
            .Where(ps => ps.Artifact != moveEvent.Piece && ps.CurrentTile != destination) // remove captured piece if any opposing there
            .Select(ps => (States.IArtifactState)ps)
            .ToList();
        updatedStates.Add(new States.PieceState(moveEvent.Piece, destination));
        var post = Veggerby.Boards.States.GameState.New(updatedStates);

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
        // Placeholder: full legality not yet implemented; cannot determine mate reliably.
        // Will be replaced once legal move generation exists.
        return false;
    }
}