using System;

namespace Veggerby.Boards.Core
{
    public class BoardException : Exception
    {
        public BoardException() { }
        public BoardException(string message) : base(message) { }
        public BoardException(string message, System.Exception inner) : base(message, inner) { }
    }
}