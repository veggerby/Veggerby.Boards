using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Go;

/// <summary>
/// Very minimal Go nomenclature placeholder: renders placements as (x,y) and pass as "pass".
/// </summary>
public sealed class GoNomenclature : IGameNomenclature
{
    /// <inheritdoc />
    public string GetPieceName(Piece piece) => piece?.Id ?? string.Empty;
    /// <inheritdoc />
    public string GetTileName(Tile tile) => tile?.Id.Replace("tile-", string.Empty) ?? string.Empty;
    /// <inheritdoc />
    public string GetDiceName(Dice dice) => dice?.Id ?? string.Empty; // not used
    /// <inheritdoc />
    public string GetPlayerName(Player player) => player?.Id switch { "black" => "Black", "white" => "White", _ => player?.Id ?? string.Empty };
    /// <inheritdoc />
    public string Describe(MovePieceGameEvent moveEvent)
    {
        if (moveEvent is null)
        {
            return string.Empty;
        }
        var toTile = moveEvent.Path?.To;
        var to = toTile is null ? string.Empty : GetTileName(toTile);
        return string.IsNullOrEmpty(to) ? string.Empty : to; // (x-y) style already produced by id stripping
    }
    /// <inheritdoc />
    public string Describe(GameState state, MovePieceGameEvent moveEvent) => Describe(moveEvent);
    /// <inheritdoc />
    public string Describe(Game game, GameState state, MovePieceGameEvent moveEvent) => Describe(moveEvent);
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