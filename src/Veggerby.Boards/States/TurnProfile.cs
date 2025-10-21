using System;
using System.Collections.Generic;
using System.Linq;

namespace Veggerby.Boards.States;

/// <summary>
/// Defines the ordered sequence of <see cref="TurnSegment"/> values that constitute a full logical turn.
/// </summary>
/// <remarks>
/// A <see cref="TurnProfile"/> is immutable and intended to be configured once at engine composition time.
/// The default profile (<see cref="Default"/>) comprises: Start → Main → End.
/// Future extensions may allow custom segment insertion (e.g. Upkeep, Combat) provided deterministic ordering
/// is preserved. Profiles never mutate <see cref="TurnState"/> directly; mutators interpret the profile to
/// transition between segments and advance the numeric turn when the last segment completes.
/// </remarks>
internal sealed class TurnProfile
{
    private readonly TurnSegment[] _orderedSegments;
    private readonly Dictionary<TurnSegment, int> _segmentOrderIndex;

    private TurnProfile(IEnumerable<TurnSegment> ordered)
    {
        _orderedSegments = ordered.ToArray();
        _segmentOrderIndex = new Dictionary<TurnSegment, int>(_orderedSegments.Length);
        for (var i = 0; i < _orderedSegments.Length; i++)
        {
            var seg = _orderedSegments[i];
            if (_segmentOrderIndex.ContainsKey(seg))
            {
                throw new ArgumentException($"Duplicate segment '{seg}' in turn profile.");
            }
            _segmentOrderIndex.Add(seg, i);
        }
        if (_orderedSegments.Length == 0)
        {
            throw new ArgumentException("Turn profile must contain at least one segment.");
        }
    }

    /// <summary>
    /// Gets the default profile: Start → Main → End.
    /// </summary>
    public static TurnProfile Default { get; } = new TurnProfile(new[] { TurnSegment.Start, TurnSegment.Main, TurnSegment.End });

    /// <summary>
    /// Returns the next segment following <paramref name="current"/> or <c>null</c> when <paramref name="current"/> is terminal.
    /// </summary>
    /// <param name="current">Current segment.</param>
    /// <returns>Next segment or <c>null</c> if the provided segment is the last segment.</returns>
    public TurnSegment? Next(TurnSegment current)
    {
        if (!_segmentOrderIndex.TryGetValue(current, out var index))
        {
            return null; // unknown segment: treat as terminal (defensive – should not occur)
        }
        var nextIndex = index + 1;
        if (nextIndex >= _orderedSegments.Length)
        {
            return null;
        }
        return _orderedSegments[nextIndex];
    }

    /// <summary>
    /// Determines whether the supplied segment is the last in the profile.
    /// </summary>
    /// <param name="segment">Segment to inspect.</param>
    /// <returns><c>true</c> if last segment.</returns>
    public bool IsLast(TurnSegment segment)
    {
        if (!_segmentOrderIndex.TryGetValue(segment, out var index))
        {
            return true; // defensive: unknown treated as terminal
        }
        return index == _orderedSegments.Length - 1;
    }
}