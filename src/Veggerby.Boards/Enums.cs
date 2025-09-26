using System;

namespace Veggerby.Boards;

/// <summary>
/// Logical composition semantics for combining multiple conditions.
/// </summary>
public enum CompositeMode
{
    /// <summary>
    /// At least one child condition must be valid.
    /// </summary>
    Any,
    /// <summary>
    /// All child conditions must evaluate <see cref="ConditionResult.Valid"/>. If any evaluate
    /// <see cref="ConditionResult.Invalid"/> the composite fails. If any evaluate
    /// <see cref="ConditionResult.Ignore"/> (and not all ignore) the composite is treated as
    /// not applicable (ignored) to allow permissive per-condition opt-out semantics.
    /// </summary>
    All,
    /// <summary>
    /// No child condition may be valid.
    /// </summary>
    None
}

/// <summary>
/// Flags specifying which players to target in ownership-based evaluations.
/// </summary>
[Flags]
public enum PlayerOption
{
    /// <summary>
    /// The active player's own pieces.
    /// </summary>
    Self = 0x1,
    /// <summary>
    /// Opponent pieces.
    /// </summary>
    Opponent = 0x2,
    /// <summary>
    /// Any player (self or opponent).
    /// </summary>
    Any = Self | Opponent
}