using System;

using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Events;

/// <summary>
/// Intent to transition the global turn timeline into the specified <see cref="TurnSegment"/>.
/// </summary>
/// <remarks>
/// Emitted (or requested) by external orchestration or future automatic rules when completing the prior segment.
/// Validation rules ensure only the next valid segment (according to <see cref="States.TurnProfile"/>) is accepted.
/// No direct state mutation occurs; a corresponding mutator interprets the event.
/// </remarks>
/// <remarks>
/// Initializes a new instance of the <see cref="BeginTurnSegmentEvent"/> class.
/// </remarks>
/// <param name="segment">Target segment to begin.</param>
public sealed class BeginTurnSegmentEvent(TurnSegment segment) : IGameEvent
{

    /// <summary>
    /// Gets the target segment.
    /// </summary>
    public TurnSegment Segment { get; } = segment;
}