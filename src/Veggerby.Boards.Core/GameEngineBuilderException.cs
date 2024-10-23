using System;
using System.Diagnostics.CodeAnalysis;

namespace Veggerby.Boards.Core;

[ExcludeFromCodeCoverage]
public class GameEngineBuilderException(string property, string message) : Exception($"{property}: {message}")
{
    public GameEngineBuilderException(string property) : this(property, "Missing value") { }
}