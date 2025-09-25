using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Internal.Attacks;
using Veggerby.Boards.Internal.Occupancy;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Internal.Acceleration;

/// <summary>
/// Acceleration context that performs no cached incremental updates beyond the underlying naive implementations.
/// </summary>
internal sealed class NaiveAccelerationContext : IAccelerationContext
{
    public IOccupancyIndex Occupancy { get; }
    public IAttackRays AttackRays { get; }

    public NaiveAccelerationContext(IOccupancyIndex occupancy, IAttackRays attackRays)
    {
        Occupancy = occupancy;
        AttackRays = attackRays;
    }

    public void OnStateTransition(GameState oldState, GameState newState, IGameEvent evt)
    {
        if (Occupancy is INaiveMutableOccupancy mutable)
        {
            mutable.BindState(newState);
        }
    }
}