using System;

namespace Veggerby.Boards.Artifacts;

/// <summary>
/// Represents a game clock artifact for tracking time-limited turns.
/// </summary>
/// <remarks>
/// GameClock is an immutable identity artifact that defines time control rules for a game.
/// The actual time tracking state is maintained in <see cref="States.ClockState"/>.
/// Time controls support various tournament formats including Fischer (increment),
/// Bronstein (delay), and move-based bonus time.
/// </remarks>
public sealed class GameClock : Artifact
{
    /// <summary>
    /// Gets the time control configuration for this clock.
    /// </summary>
    public TimeControl Control
    {
        get;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GameClock"/> class.
    /// </summary>
    /// <param name="id">Unique identifier for this clock.</param>
    /// <param name="control">Time control configuration.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="control"/> is null.</exception>
    public GameClock(string id, TimeControl control) : base(id)
    {
        ArgumentNullException.ThrowIfNull(control, nameof(control));

        Control = control;
    }
}
