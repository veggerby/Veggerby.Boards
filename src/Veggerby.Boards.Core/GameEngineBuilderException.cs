using System;
using System.Diagnostics.CodeAnalysis;

namespace Veggerby.Boards.Core
{
    [ExcludeFromCodeCoverage]
    public class GameEngineBuilderException : Exception
    {
        public GameEngineBuilderException(string property) : this(property, "Missing value") { }
        public GameEngineBuilderException(string property, string message) : base($"{property}: {message}") { }
    }
}