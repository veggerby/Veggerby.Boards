namespace Veggerby.Boards.Core
{
    public class BoardException : System.Exception
    {
        public BoardException() { }
        public BoardException(string message) : base(message) { }
        public BoardException(string message, System.Exception inner) : base(message, inner) { }
    }
}