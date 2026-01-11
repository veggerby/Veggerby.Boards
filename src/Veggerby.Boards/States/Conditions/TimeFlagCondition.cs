using System.Linq;

using Veggerby.Boards.Artifacts;

namespace Veggerby.Boards.States.Conditions;

/// <summary>
/// Condition that detects when any player has run out of time.
/// </summary>
/// <remarks>
/// This condition scans all clock states to check if any player has expired time.
/// It should typically be used with a TimeFlagStateMutator to trigger game termination.
/// </remarks>
public sealed class TimeFlagCondition : IGameStateCondition
{
    private readonly Game _game;

    /// <summary>
    /// Initializes a new instance of the <see cref="TimeFlagCondition"/> class.
    /// </summary>
    /// <param name="game">The game instance.</param>
    public TimeFlagCondition(Game game)
    {
        ArgumentNullException.ThrowIfNull(game);

        _game = game;
    }

    /// <inheritdoc />
    public ConditionResponse Evaluate(GameState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        var clockState = state.GetStates<ClockState>().FirstOrDefault();

        if (clockState == null)
        {
            return ConditionResponse.Ignore("No clock configured");
        }

        if (_game.Artifacts.OfType<Player>().Any(player => clockState.IsTimeExpired(player)))
        {
            return ConditionResponse.Valid;
        }

        return ConditionResponse.Ignore("All players have time remaining");
    }
}
