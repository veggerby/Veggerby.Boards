namespace Veggerby.Boards.Core.Contracts.Models.Navigation
{
    public class TileException : System.Exception
    {
        public TileException() { }
        public TileException( string message ) : base( message ) { }
        public TileException( string message, System.Exception inner ) : base( message, inner ) { }
    }
    
    public class TilePathException : System.Exception
    {
        public TilePathException() { }
        public TilePathException( string message ) : base( message ) { }
        public TilePathException( string message, System.Exception inner ) : base( message, inner ) { }
    }
}