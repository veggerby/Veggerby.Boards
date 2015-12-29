namespace Veggerby.Boards.Core.Contracts.Models.Navigation
{
    public class TileException : System.Exception
    {
        public TileException() { }
        public TileException( string message ) : base( message ) { }
        public TileException( string message, System.Exception inner ) : base( message, inner ) { }
    }
}