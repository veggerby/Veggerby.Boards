using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;

namespace Veggerby.Boards.Internal.Bitboards;

/// <summary>
/// Immutable layout mapping for a board with at most 64 tiles assigning each tile a stable bit index.
/// </summary>
/// <remarks>
/// Tile ordering is deterministic (ordinal by tile id) to ensure reproducible bit assignments across builds.
/// </remarks>
internal sealed class BoardBitboardLayout
{
    private readonly Dictionary<Tile, int> _tileToIndex;
    private readonly Tile[] _indexToTile;
    private readonly Bitboard64[] _singleTileMasks;

    public BoardBitboardLayout(Board board)
    {
        ArgumentNullException.ThrowIfNull(board);
        // Collect tiles into a temporary list (<=64) then ensure deterministic ordinal ordering.
        // Avoid LINQ OrderBy allocation; for small N an insertion sort is cheaper and allocation-free.
        var temp = new List<Tile>(capacity: 64);
        foreach (var t in board.Tiles)
        {
            temp.Add(t);
        }

        // Check if already sorted by Id to skip sort.
        var alreadySorted = true;
        for (int i = 1; i < temp.Count; i++)
        {
            if (string.CompareOrdinal(temp[i - 1].Id, temp[i].Id) > 0)
            {
                alreadySorted = false;
                break;
            }
        }

        if (!alreadySorted)
        {
            // Insertion sort (n <= 64) minimal overhead.
            for (int i = 1; i < temp.Count; i++)
            {
                var current = temp[i];
                var j = i - 1;
                while (j >= 0 && string.CompareOrdinal(temp[j].Id, current.Id) > 0)
                {
                    temp[j + 1] = temp[j];
                    j--;
                }
                temp[j + 1] = current;
            }
        }

        var tiles = temp.ToArray();
        if (tiles.Length > 64)
        {
            throw new ArgumentException(ExceptionMessages.BoardTooLargeForBitboard);
        }
        _tileToIndex = new Dictionary<Tile, int>(tiles.Length);
        _indexToTile = new Tile[tiles.Length];
        _singleTileMasks = new Bitboard64[tiles.Length];
        for (int i = 0; i < tiles.Length; i++)
        {
            _tileToIndex[tiles[i]] = i;
            _indexToTile[i] = tiles[i];
            _singleTileMasks[i] = new Bitboard64(1UL << i);
        }
    }

    public int TileCount => _indexToTile.Length;

    public bool TryGetIndex(Tile tile, out int index) => _tileToIndex.TryGetValue(tile, out index);

    public Tile GetTile(int index) => _indexToTile[index];

    public Bitboard64 this[Tile tile]
    {
        get
        {
            return _tileToIndex.TryGetValue(tile, out var idx) ? _singleTileMasks[idx] : new Bitboard64(0);
        }
    }

    public Bitboard64 GetMask(int index) => _singleTileMasks[index];
}