using System;

namespace Veggerby.Boards.Core
{
    public enum CompositeMode
    {
        Any,
        All
    }

    [Flags]
    public enum PlayerOption
    {
        Self = 0x1,
        Opponent = 0x3,
        Any = Self ^ Opponent
    }
}