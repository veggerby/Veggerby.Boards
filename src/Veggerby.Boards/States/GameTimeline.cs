using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Veggerby.Boards.States;

/// <summary>
/// Immutable zipper structure representing navigable game history: a sequence of past states, a present state, and optional future states.
/// </summary>
/// <remarks>
/// Operations (<see cref="Push"/>, <see cref="Undo"/>, <see cref="Redo"/>) yield new timeline instances; underlying <see cref="GameState"/> objects are immutable and shared.
/// Past is stored with oldest at index 0 and most recent immediately before <see cref="Present"/>. Future ordering: earliest redo candidate first.
/// When a new state is pushed after undoing (i.e., non-empty future), the future branch is discarded (standard timeline semantics).
/// </remarks>
public sealed class GameTimeline
{
    private GameTimeline(ImmutableArray<GameState> past, GameState present, ImmutableArray<GameState> future)
    {
        Past = past;
        Present = present ?? throw new ArgumentNullException(nameof(present));
        Future = future;
    }

    /// <summary>
    /// Gets the immutable sequence of past states (oldest -> newest).
    /// </summary>
    public ImmutableArray<GameState> Past { get; }

    /// <summary>
    /// Gets the current state (cursor position).
    /// </summary>
    public GameState Present { get; }

    /// <summary>
    /// Gets the immutable sequence of future states available for redo (earliest -> latest).
    /// </summary>
    public ImmutableArray<GameState> Future { get; }

    /// <summary>
    /// Creates a new timeline seeded with an initial present state.
    /// </summary>
    public static GameTimeline Create(GameState initial)
    {
        if (initial is null)
        {
            throw new ArgumentNullException(nameof(initial));
        }
        return new GameTimeline(ImmutableArray<GameState>.Empty, initial, ImmutableArray<GameState>.Empty);
    }

    /// <summary>
    /// Appends a successor state as the new present, shifting the existing present into past and clearing any future branch.
    /// </summary>
    public GameTimeline Push(GameState next)
    {
        if (next is null)
        {
            throw new ArgumentNullException(nameof(next));
        }
        var newPast = Past.Add(Present);
        return new GameTimeline(newPast, next, ImmutableArray<GameState>.Empty);
    }

    /// <summary>
    /// Moves the cursor one step backward if possible, returning a new timeline; otherwise returns this (no change) when at earliest state.
    /// </summary>
    public GameTimeline Undo()
    {
        if (Past.IsEmpty)
        {
            return this; // nothing to undo
        }
        var newPresent = Past[^1];
        var newPast = Past.RemoveAt(Past.Length - 1);
        var newFuture = Future.Insert(0, Present);
        return new GameTimeline(newPast, newPresent, newFuture);
    }

    /// <summary>
    /// Moves the cursor one step forward if possible (redo), otherwise returns this when no future states exist.
    /// </summary>
    public GameTimeline Redo()
    {
        if (Future.IsEmpty)
        {
            return this; // nothing to redo
        }
        var newPresent = Future[0];
        var newFuture = Future.RemoveAt(0);
        var newPast = Past.Add(Present);
        return new GameTimeline(newPast, newPresent, newFuture);
    }

    /// <summary>
    /// Enumerates the full linear history if needed (past + present + future) without allocating new arrays (deferred).
    /// </summary>
    public IEnumerable<GameState> EnumerateAll()
    {
        foreach (var p in Past)
        {
            yield return p;
        }
        yield return Present;
        foreach (var f in Future)
        {
            yield return f;
        }
    }
}