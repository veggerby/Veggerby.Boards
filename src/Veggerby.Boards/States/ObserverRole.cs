namespace Veggerby.Boards.States;

/// <summary>
/// Defines the access level for observers viewing a game without direct participation.
/// </summary>
/// <remarks>
/// Observer roles control visibility permissions for spectators, analysts, or administrative views.
/// Different roles reveal different subsets of game state based on tournament rules or analysis needs.
/// </remarks>
public enum ObserverRole
{
    /// <summary>
    /// Full visibility including all hidden state (e.g., admin, arbiter, post-game analysis).
    /// </summary>
    Full,

    /// <summary>
    /// Public state only, no private or hidden information (e.g., live tournament spectator).
    /// </summary>
    Limited,

    /// <summary>
    /// View from a specific player's perspective (e.g., training replay, coaching view).
    /// </summary>
    /// <remarks>
    /// When using this role, the target player must be specified separately in the projection context.
    /// </remarks>
    PlayerPerspective
}
