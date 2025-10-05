using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;

namespace Veggerby.Boards.Internal.Layout;

/// <summary>
/// Layout describing stable indices for players and pieces plus helper to construct immutable snapshots of per-piece tile indices.
/// </summary>
internal sealed class PieceMapLayout
{
    public Player[] Players { get; }
    public Piece[] Pieces { get; }
    private readonly Dictionary<Player, int> _playerToIndex;
    private readonly Dictionary<Piece, int> _pieceToIndex;
    public int PlayerCount => Players.Length;
    public int PieceCount => Pieces.Length;

    private PieceMapLayout(Player[] players, Piece[] pieces, Dictionary<Player, int> p2i, Dictionary<Piece, int> pc2i)
    {
        Players = players;
        Pieces = pieces;
        _playerToIndex = p2i;
        _pieceToIndex = pc2i;
    }

    public static PieceMapLayout Build(Game game)
    {
        ArgumentNullException.ThrowIfNull(game);
        var players = game
            .Players
            .OrderBy(p => p.Id, StringComparer.Ordinal)
            .ToArray();

        var p2i = new Dictionary<Player, int>(players.Length);
        for (int i = 0; i < players.Length; i++)
        {
            p2i[players[i]] = i;
        }

        var pieces = game
            .Artifacts
            .OfType<Piece>()
            .OrderBy(p => p.Id, StringComparer.Ordinal)
            .ToArray();

        var pc2i = new Dictionary<Piece, int>(pieces.Length);
        for (int i = 0; i < pieces.Length; i++)
        {
            pc2i[pieces[i]] = i;
        }

        return new PieceMapLayout(players, pieces, p2i, pc2i);
    }

    public bool TryGetPlayerIndex(Player player, out int index) => _playerToIndex.TryGetValue(player, out index);
    public bool TryGetPieceIndex(Piece piece, out int index) => _pieceToIndex.TryGetValue(piece, out index);
}