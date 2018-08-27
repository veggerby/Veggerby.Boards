using System;
using System.Diagnostics.CodeAnalysis;

namespace Veggerby.Boards.Core
{
    [ExcludeFromCodeCoverage]
    public class BoardException : Exception
    {
        public BoardException() { }
        public BoardException(string message) : base(message) { }
        public BoardException(string message, System.Exception inner) : base(message, inner) { }
    }
}