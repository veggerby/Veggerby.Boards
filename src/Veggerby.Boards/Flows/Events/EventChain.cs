using System;
using System.Collections.Generic;

namespace Veggerby.Boards.Flows.Events;

/// <summary>
/// Immutable persistent chain of game events supporting O(1) append via tail segmentation.
/// </summary>
public sealed class EventChain : IEnumerable<IGameEvent>
{
    internal sealed class Segment
    {
        internal readonly IGameEvent[] Events;
        internal readonly Segment? Previous;
        internal Segment(IGameEvent[] events, Segment? previous)
        {
            Events = events;
            Previous = previous;
        }
    }

    private readonly Segment _tail;
    private readonly int _count;
    private readonly int _segmentCount;

    private EventChain(Segment tail, int count, int segmentCount)
    {
        _tail = tail;
        _count = count;
        _segmentCount = segmentCount;
    }

    /// <summary>
    /// Gets the number of events in the chain.
    /// </summary>
    public int Count => _count;

    /// <summary>
    /// Creates a new empty chain.
    /// </summary>
    public static EventChain Empty { get; } = new EventChain(new Segment(Array.Empty<IGameEvent>(), null), 0, 1);

    /// <summary>
    /// Appends a single event producing a new chain instance.
    /// </summary>
    public EventChain Append(IGameEvent evt)
    {
        ArgumentNullException.ThrowIfNull(evt);
        // Single-event segment appended; structural sharing of previous tail preserved.
        var segment = new Segment(new[] { evt }, _tail);
        return new EventChain(segment, _count + 1, _segmentCount + 1);
    }

    /// <summary>
    /// Batch appends events producing a new chain instance.
    /// </summary>
    public EventChain AppendRange(IEnumerable<IGameEvent> events)
    {
        ArgumentNullException.ThrowIfNull(events);
        IGameEvent[] array;
        if (events is IGameEvent[] direct)
        {
            array = direct.Length == 0 ? Array.Empty<IGameEvent>() : (IGameEvent[])direct.Clone();
        }
        else if (events is ICollection<IGameEvent> coll)
        {
            if (coll.Count == 0)
            {
                return this;
            }

            array = new IGameEvent[coll.Count];
            coll.CopyTo(array, 0);
        }
        else
        {
            // Fallback enumerate once
            var list = new List<IGameEvent>();
            foreach (var e in events)
            {
                list.Add(e);
            }

            if (list.Count == 0)
            {
                return this;
            }

            array = list.ToArray();
        }

        var segment = new Segment(array, _tail);
        return new EventChain(segment, _count + array.Length, _segmentCount + 1);
    }
    /// <summary>
    /// Returns an enumerator iterating events in chronological order.
    /// </summary>
    // Internal enumerator exposure; public enumeration flows through explicit interface implementations.
    private Enumerator GetEnumeratorInternal() => new Enumerator(_tail, _segmentCount);

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumeratorInternal();

    IEnumerator<IGameEvent> IEnumerable<IGameEvent>.GetEnumerator() => GetEnumeratorInternal();

    /// <summary>
    /// Struct enumerator to avoid heap allocations.
    /// </summary>
    internal struct Enumerator : IEnumerator<IGameEvent>
    {
        private readonly Segment[] _segments;
        private int _segmentIndex;
        private int _eventIndex;
        private IGameEvent? _current;

        internal Enumerator(Segment tail, int segmentCount)
        {
            _segments = new Segment[segmentCount];
            var cur = tail;
            var idx = 0;
            while (cur is not null && idx < segmentCount)
            {
                _segments[idx++] = cur;
                cur = cur.Previous;
            }

            _segmentIndex = idx - 1; // start from last filled (chronological reversal)
            _eventIndex = -1;
            _current = null;
        }

        /// <summary>
        /// Gets the current event in iteration.
        /// </summary>
        public IGameEvent Current => _current ?? throw new InvalidOperationException("Enumerator not positioned.");
        object System.Collections.IEnumerator.Current => Current;

        /// <summary>
        /// Advances to the next event.
        /// </summary>
        public bool MoveNext()
        {
            if (_segmentIndex < 0)
            {
                return false;
            }

            var seg = _segments[_segmentIndex];
            _eventIndex++;
            if (_eventIndex >= seg.Events.Length)
            {
                _segmentIndex--;
                _eventIndex = 0;
                if (_segmentIndex < 0)
                {
                    _current = null;
                    return false;
                }

                seg = _segments[_segmentIndex];
            }

            _current = seg.Events[_eventIndex];
            return true;
        }

        /// <summary>
        /// Reset is not supported for this enumerator.
        /// </summary>
        public void Reset() => throw new NotSupportedException();

        /// <summary>
        /// Disposes enumerator (no resources held).
        /// </summary>
        public void Dispose()
        {
        }
    }
}