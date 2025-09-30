using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards;

/// <summary>
/// Provides human-friendly, game-specific naming for core artifact concepts (pieces, tiles, dice) and events.
/// </summary>
/// <remarks>
/// Implementations must be pure (no IO) and deterministic. They should not inspect mutable state beyond
/// the provided artifacts or events. This is a presentation layer helper and MUST NOT influence engine logic.
/// </remarks>
public interface IGameNomenclature
{
    /// <summary>Gets a friendly name for a piece artifact identifier.</summary>
    string GetPieceName(Piece piece);

    /// <summary>Gets a friendly name for a tile artifact identifier.</summary>
    string GetTileName(Tile tile);

    /// <summary>Gets a friendly name for a dice artifact.</summary>
    string GetDiceName(Dice dice);

    /// <summary>Gets a friendly name for a player artifact.</summary>
    string GetPlayerName(Player player);

    /// <summary>Describes a move piece event in friendly textual form.</summary>
    string Describe(MovePieceGameEvent moveEvent);

    /// <summary>
    /// Describes a move using the supplied <see cref="GameState"/> for contextual details (e.g., captures in chess).
    /// Default implementation delegates to the legacy overload for backward compatibility.
    /// </summary>
    /// <param name="state">Pre-move game state (the event has not yet been applied).</param>
    /// <param name="moveEvent">The move event.</param>
    /// <returns>Contextual description.</returns>
    public virtual string Describe(GameState state, MovePieceGameEvent moveEvent) => Describe(moveEvent);

    /// <summary>
    /// Describes a move with full game context (state + topology) enabling richer domain specific notation (e.g., SAN disambiguation).
    /// Default implementation delegates to the state-only overload for backward compatibility.
    /// </summary>
    /// <param name="game">Game definition (board, artifacts).</param>
    /// <param name="state">Pre-move game state.</param>
    /// <param name="moveEvent">Move event.</param>
    /// <returns>Contextual description.</returns>
    public virtual string Describe(Game game, GameState state, MovePieceGameEvent moveEvent) => Describe(state, moveEvent);

    /// <summary>Describes a roll dice event in friendly textual form.</summary>
    string Describe<TValue>(RollDiceGameEvent<TValue> rollDiceEvent);
}

/// <summary>
/// Fallback generic nomenclature used when no domain specific implementation is available.
/// </summary>
/// <summary>
/// Generic nomenclature that mirrors raw identifiers. Used when no domain specific nomenclature is available.
/// </summary>
public sealed class GenericNomenclature : IGameNomenclature
{
    /// <inheritdoc />
    public string GetPieceName(Piece piece) => piece?.Id ?? string.Empty;
    /// <inheritdoc />
    public string GetTileName(Tile tile) => tile?.Id ?? string.Empty;
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
        var from = moveEvent.Path?.From?.Id ?? "?";
        var to = moveEvent.Path?.To?.Id ?? "?";
        return $"move {moveEvent.Piece.Id} {from}->{to}";
    }
    /// <inheritdoc />
    public string Describe<TValue>(RollDiceGameEvent<TValue> rollDiceEvent)
    {
        if (rollDiceEvent is null)
        {
            return string.Empty;
        }
        return $"roll[{string.Join(',', rollDiceEvent.NewDiceStates.Select(s => s.CurrentValue))}]";
    }
}