using System;

namespace Veggerby.Boards.Core;

public enum CompositeMode
{
    Any,
    All,
    None
}

[Flags]
public enum PlayerOption
{
    Self = 0x1,
    Opponent = 0x2,
    Any = Self | Opponent
}