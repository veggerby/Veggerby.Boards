using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Events;

namespace Veggerby.Boards.Backgammon;

/// <summary>
/// Backgammon nomenclature mapping tiles to point numbers and simple piece/dice descriptions.
/// </summary>
public sealed class BackgammonNomenclature : IGameNomenclature
{
    /// <inheritdoc />
    public string GetPieceName(Piece piece) => piece?.Id ?? string.Empty;

    /// <inheritdoc />
    public string GetTileName(Tile tile)
    {
        if (tile is null)
        {
            return string.Empty;
        }
        // Expect tile ids like tile-point-1 .. tile-point-24 (example assumption); strip prefix
        if (tile.Id.StartsWith("tile-"))
        {
            return tile.Id[5..];
        }
        return tile.Id;
    }

    /// <inheritdoc />
    public string GetDiceName(Dice dice) => dice?.Id ?? string.Empty;

    /// <inheritdoc />
    public string GetPlayerName(Player player) => player?.Id ?? string.Empty;

    /// <inheritdoc />
    public string Describe(MovePieceGameEvent moveEvent)
    {
        if (moveEvent is null)
        {
            return string.Empty;
        }
    var fromTile = moveEvent.Path?.From;
    var toTile = moveEvent.Path?.To;
    var from = fromTile is null ? string.Empty : GetTileName(fromTile);
    var to = toTile is null ? string.Empty : GetTileName(toTile);
        return $"move {moveEvent.Piece.Id} {from}->{to}";
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
}