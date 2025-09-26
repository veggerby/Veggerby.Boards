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

    /// <summary>
    /// Stable ordered player array (ordinal by id) matching indices used in occupancy bitboards.
    /// </summary>
    public Player[] Players { get; }

    /// <summary>
    /// Stable ordered piece array (ordinal by id) matching indices used for optional per-piece masks.
    /// </summary>
    public Piece[] Pieces { get; }

    private BitboardLayout(Dictionary<Player, int> p2i, Dictionary<Piece, int> piece2i, Player[] players, Piece[] pieces)
    {
        _playerToIndex = p2i;
        _pieceToIndex = piece2i;
        Players = players;
        Pieces = pieces;
    }

    public int PlayerCount => Players.Length;
    public int PieceCount => Pieces.Length;

    public static BitboardLayout Build(Game game)
    {
        ArgumentNullException.ThrowIfNull(game);

        var players = new List<Player>(game.Players);
        players.Sort((a, b) => string.Compare(a.Id, b.Id, StringComparison.Ordinal));
        var playerArray = players.ToArray();
        var p2i = new Dictionary<Player, int>(playerArray.Length);
        for (int i = 0; i < playerArray.Length; i++)
        {
            p2i[playerArray[i]] = i;
        }

        // Include all pieces (ordered by id) – future filtering possible for piece-type specific bitboards.
        var pieces = game
            .Artifacts
            .OfType<Piece>()
            .OrderBy(p => p.Id, StringComparer.Ordinal)
            .ToList();

        var pieceArray = pieces.ToArray();
        var piece2i = new Dictionary<Piece, int>(pieceArray.Length);
        for (int i = 0; i < pieceArray.Length; i++)
        {
            piece2i[pieceArray[i]] = i;
        }

        return new BitboardLayout(p2i, piece2i, playerArray, pieceArray);
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