using System;

namespace Veggerby.Boards.Internal.Attacks;

internal sealed class AttackGeneratorServices(SlidingAttackGenerator sliding)
{
    public SlidingAttackGenerator Sliding { get; } = sliding ?? throw new ArgumentNullException(nameof(sliding));
}