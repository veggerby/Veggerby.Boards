using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;

namespace Veggerby.Boards.Internal.Layout;

/// <summary>
/// Stable index mapping for players and pieces used by bitboard snapshots. Mirrors <see cref="PieceMapLayout"/> but
/// scoped for bitboard occupancy masks (per-player) and future piece-type masks.
/// </summary>
internal sealed class BitboardLayout
{
    private readonly Dictionary<Player, int> _playerToIndex;
    private readonly Dictionary<Piece, int> _pieceToIndex;

    private BitboardLayout(Dictionary<Player, int> p2i, Dictionary<Piece, int> piece2i)
    {
        _playerToIndex = p2i;
        _pieceToIndex = piece2i;
    }

    public int PlayerCount => _playerToIndex.Count;
    public int PieceCount => _pieceToIndex.Count;

    public static BitboardLayout Build(Game game)
    {
        ArgumentNullException.ThrowIfNull(game);

        var players = new List<Player>(game.Players);
        players.Sort((a, b) => string.Compare(a.Id, b.Id, StringComparison.Ordinal));
        var p2i = new Dictionary<Player, int>(players.Count);
        for (int i = 0; i < players.Count; i++)
        {
            p2i[players[i]] = i;
        }

        // Include all pieces (ordered by id) – future filtering possible for piece-type specific bitboards.
        var pieces = game
            .Artifacts
            .OfType<Piece>()
            .OrderBy(p => p.Id, StringComparer.Ordinal)
            .ToList();

        var piece2i = new Dictionary<Piece, int>(pieces.Count);
        for (int i = 0; i < pieces.Count; i++)
        {
            piece2i[pieces[i]] = i;
        }

        return new BitboardLayout(p2i, piece2i);
    }

    public bool TryGetPlayerIndex(Player player, out int index)
    {
        return _playerToIndex.TryGetValue(player, out index);
    }

    public bool TryGetPieceIndex(Piece piece, out int index)
    {
        return _pieceToIndex.TryGetValue(piece, out index);
    }
}