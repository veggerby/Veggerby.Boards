using System;
using System.Collections.Generic;

using Veggerby.Boards.Flows.Events;

namespace Veggerby.Boards.States;

/// <summary>
/// Extends GameProgress with history navigation capabilities for undo/redo and timeline exploration.
/// </summary>
/// <remarks>
/// <see cref="GameHistory"/> maintains an immutable timeline of game progression snapshots,
/// enabling efficient backward/forward navigation without violating determinism guarantees.
/// Each history node captures both the <see cref="GameProgress"/> state and the triggering event.
/// </remarks>
public sealed class GameHistory
{
    private readonly List<HistoryNode> _timeline;
    private readonly int _currentIndex;

    /// <summary>
    /// Initializes a new instance of the <see cref="GameHistory"/> class with an initial progress state.
    /// </summary>
    /// <param name="initialProgress">The initial game progress to start the history.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="initialProgress"/> is null.</exception>
    public GameHistory(GameProgress initialProgress)
    {
        ArgumentNullException.ThrowIfNull(initialProgress);

        _timeline = new List<HistoryNode>
        {
            new HistoryNode(initialProgress, new NullGameEvent())
        };
        _currentIndex = 0;
        Current = initialProgress;
    }

    /// <summary>
    /// Internal constructor for creating new history instances during navigation.
    /// </summary>
    private GameHistory(List<HistoryNode> timeline, int currentIndex)
    {
        _timeline = timeline;
        _currentIndex = currentIndex;
        Current = _timeline[_currentIndex].Progress;
    }

    /// <summary>
    /// Gets the current game progress at the active position in history.
    /// </summary>
    public GameProgress Current
    {
        get;
    }

    /// <summary>
    /// Gets the current index position in the timeline (zero-based).
    /// </summary>
    public int CurrentIndex => _currentIndex;

    /// <summary>
    /// Gets the total length of the timeline.
    /// </summary>
    public int Length => _timeline.Count;

    /// <summary>
    /// Gets a value indicating whether undo operation is possible (not at start of history).
    /// </summary>
    public bool CanUndo => _currentIndex > 0;

    /// <summary>
    /// Gets a value indicating whether redo operation is possible (not at end of timeline).
    /// </summary>
    public bool CanRedo => _currentIndex < _timeline.Count - 1;

    /// <summary>
    /// Moves back one step in history, producing a new <see cref="GameHistory"/> instance.
    /// </summary>
    /// <returns>A new history instance positioned at the previous state.</returns>
    /// <exception cref="InvalidOperationException">Thrown when already at the start of history.</exception>
    public GameHistory Undo()
    {
        if (!CanUndo)
        {
            throw new InvalidOperationException("Cannot undo: at start of history");
        }

        return new GameHistory(_timeline, _currentIndex - 1);
    }

    /// <summary>
    /// Moves forward one step in history, producing a new <see cref="GameHistory"/> instance.
    /// </summary>
    /// <returns>A new history instance positioned at the next state.</returns>
    /// <exception cref="InvalidOperationException">Thrown when already at the end of timeline.</exception>
    public GameHistory Redo()
    {
        if (!CanRedo)
        {
            throw new InvalidOperationException("Cannot redo: at end of history");
        }

        return new GameHistory(_timeline, _currentIndex + 1);
    }

    /// <summary>
    /// Applies a new event and appends to history, producing a new <see cref="GameHistory"/> instance.
    /// </summary>
    /// <param name="event">The game event to apply.</param>
    /// <returns>A new history instance with the event applied.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="event"/> is null.</exception>
    /// <remarks>
    /// If not at the end of the timeline when applying a new event, the timeline is truncated
    /// at the current position before appending the new event (discarding alternate future).
    /// This maintains a linear history without automatic branching.
    /// </remarks>
    public GameHistory Apply(IGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        var newProgress = Current.HandleEvent(@event);

        // Create new timeline by copying nodes up to current position, then appending new node
        var newTimeline = new List<HistoryNode>(_currentIndex + 2);
        for (var i = 0; i <= _currentIndex; i++)
        {
            newTimeline.Add(_timeline[i]);
        }

        newTimeline.Add(new HistoryNode(newProgress, @event));

        return new GameHistory(newTimeline, _currentIndex + 1);
    }

    /// <summary>
    /// Jumps to a specific point in history by index, producing a new <see cref="GameHistory"/> instance.
    /// </summary>
    /// <param name="index">The zero-based index to navigate to.</param>
    /// <returns>A new history instance positioned at the specified index.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="index"/> is out of bounds.</exception>
    public GameHistory GoTo(int index)
    {
        if (index < 0 || index >= _timeline.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index), $"Index must be between 0 and {_timeline.Count - 1}");
        }

        return new GameHistory(_timeline, index);
    }

    /// <summary>
    /// Gets all events applied to reach the current state.
    /// </summary>
    /// <returns>A read-only list of events in chronological order.</returns>
    /// <remarks>
    /// The first event in the list is always a <see cref="NullGameEvent"/> representing the initial state.
    /// Subsequent events are those explicitly applied via <see cref="Apply"/>.
    /// </remarks>
    public IReadOnlyList<IGameEvent> GetEventHistory()
    {
        var events = new List<IGameEvent>(_currentIndex + 1);
        for (var i = 0; i <= _currentIndex; i++)
        {
            events.Add(_timeline[i].Event);
        }

        return events.AsReadOnly();
    }

    /// <summary>
    /// Gets a read-only view of all timeline nodes up to the current position.
    /// </summary>
    /// <returns>A read-only list of history nodes.</returns>
    internal IReadOnlyList<HistoryNode> GetTimeline()
    {
        return _timeline.AsReadOnly();
    }
}

/// <summary>
/// Represents a single node in the history timeline, capturing both the game state and the event that led to it.
/// </summary>
/// <param name="Progress">The game progress snapshot at this point in history.</param>
/// <param name="Event">The event that was applied to reach this state.</param>
internal sealed record HistoryNode(GameProgress Progress, IGameEvent Event);
