using Veggerby.Boards.Flows.Events;

namespace Veggerby.Boards.Flows.LegalMoveGeneration;

/// <summary>
/// Result of move validation with structured diagnostics.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="MoveValidation"/> provides a lightweight, allocation-friendly result for
/// validating individual moves. It captures whether the move is legal and, if not, provides
/// structured diagnostic information.
/// </para>
/// <para>
/// Use the static factory methods <see cref="Legal"/> and <see cref="Illegal"/> to create
/// instances with appropriate defaults.
/// </para>
/// </remarks>
/// <param name="IsLegal">True if the event is legal and can be applied; false otherwise.</param>
/// <param name="Event">The event being validated.</param>
/// <param name="Reason">The reason for rejection (None if legal).</param>
/// <param name="Explanation">Human-readable explanation of the rejection (null if legal or no explanation available).</param>
public sealed record MoveValidation(bool IsLegal, IGameEvent Event, RejectionReason Reason, string? Explanation)
{
    /// <summary>
    /// Creates a validation result indicating the move is legal.
    /// </summary>
    /// <param name="event">The legal event.</param>
    /// <returns>A legal validation result.</returns>
    public static MoveValidation Legal(IGameEvent @event)
    {
        return new MoveValidation(true, @event, RejectionReason.None, null);
    }

    /// <summary>
    /// Creates a validation result indicating the move is illegal.
    /// </summary>
    /// <param name="event">The illegal event.</param>
    /// <param name="reason">The structured rejection reason.</param>
    /// <param name="explanation">Optional human-readable explanation.</param>
    /// <returns>An illegal validation result.</returns>
    public static MoveValidation Illegal(IGameEvent @event, RejectionReason reason, string? explanation = null)
    {
        return new MoveValidation(false, @event, reason, explanation);
    }
}
