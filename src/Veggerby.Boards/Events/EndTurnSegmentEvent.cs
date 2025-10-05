using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Events;

/// <summary>
/// Intent signaling the completion of the specified <see cref="TurnSegment"/> within the active turn.
/// </summary>
/// <remarks>
/// This precedes either a transition to the next segment (if any) via <see cref="BeginTurnSegmentEvent"/> or a full
/// turn advancement when the completed segment is the terminal segment in the active <see cref="TurnProfile"/>.
/// </remarks>
/// <remarks>
/// Initializes a new instance of the <see cref="EndTurnSegmentEvent"/> class.
/// </remarks>
/// <param name="segment">Segment being completed.</param>
public sealed class EndTurnSegmentEvent(TurnSegment segment) : IGameEvent
{

    /// <summary>
    /// Gets the segment being ended.
    /// </summary>
    public TurnSegment Segment { get; } = segment;
}