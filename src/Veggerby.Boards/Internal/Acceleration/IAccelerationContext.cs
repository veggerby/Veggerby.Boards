using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Internal.Attacks;
using Veggerby.Boards.Internal.Occupancy;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Internal.Acceleration;

/// <summary>
/// Aggregates acceleration-layer capabilities (occupancy, attack rays) and provides lifecycle hook for state transitions.
/// </summary>
internal interface IAccelerationContext
{
    IOccupancyIndex Occupancy { get; }
    IAttackRays AttackRays { get; }
    void OnStateTransition(GameState previous, GameState current, IGameEvent evt);
}