#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Veggerby.Boards.Checkers;

/// <summary>
/// Constant identifiers for checkers artifacts (players, pieces, tiles).
/// </summary>
public static class CheckersIds
{
    /// <summary>
    /// Player identifiers.
    /// </summary>
    public static class Players
    {
        /// <summary>Black player (moves first, occupies rows 1-3 initially).</summary>
        public const string Black = "black";

        /// <summary>White player (moves second, occupies rows 6-8 initially).</summary>
        public const string White = "white";
    }

    /// <summary>
    /// Piece identifiers.
    /// </summary>
    public static class Pieces
    {
        // Black pieces
        public const string BlackPiece1 = "black-piece-1";
        public const string BlackPiece2 = "black-piece-2";
        public const string BlackPiece3 = "black-piece-3";
        public const string BlackPiece4 = "black-piece-4";
        public const string BlackPiece5 = "black-piece-5";
        public const string BlackPiece6 = "black-piece-6";
        public const string BlackPiece7 = "black-piece-7";
        public const string BlackPiece8 = "black-piece-8";
        public const string BlackPiece9 = "black-piece-9";
        public const string BlackPiece10 = "black-piece-10";
        public const string BlackPiece11 = "black-piece-11";
        public const string BlackPiece12 = "black-piece-12";

        // White pieces
        public const string WhitePiece1 = "white-piece-1";
        public const string WhitePiece2 = "white-piece-2";
        public const string WhitePiece3 = "white-piece-3";
        public const string WhitePiece4 = "white-piece-4";
        public const string WhitePiece5 = "white-piece-5";
        public const string WhitePiece6 = "white-piece-6";
        public const string WhitePiece7 = "white-piece-7";
        public const string WhitePiece8 = "white-piece-8";
        public const string WhitePiece9 = "white-piece-9";
        public const string WhitePiece10 = "white-piece-10";
        public const string WhitePiece11 = "white-piece-11";
        public const string WhitePiece12 = "white-piece-12";
    }

    /// <summary>
    /// Tile identifiers for dark squares only (32 playable squares on standard 8x8 board).
    /// Tiles are numbered 1-32 following standard checkers notation.
    /// </summary>
    public static class Tiles
    {
        // Row 1 (from white perspective, bottom)
        public const string Tile1 = "tile-1";
        public const string Tile2 = "tile-2";
        public const string Tile3 = "tile-3";
        public const string Tile4 = "tile-4";

        // Row 2
        public const string Tile5 = "tile-5";
        public const string Tile6 = "tile-6";
        public const string Tile7 = "tile-7";
        public const string Tile8 = "tile-8";

        // Row 3
        public const string Tile9 = "tile-9";
        public const string Tile10 = "tile-10";
        public const string Tile11 = "tile-11";
        public const string Tile12 = "tile-12";

        // Row 4
        public const string Tile13 = "tile-13";
        public const string Tile14 = "tile-14";
        public const string Tile15 = "tile-15";
        public const string Tile16 = "tile-16";

        // Row 5
        public const string Tile17 = "tile-17";
        public const string Tile18 = "tile-18";
        public const string Tile19 = "tile-19";
        public const string Tile20 = "tile-20";

        // Row 6
        public const string Tile21 = "tile-21";
        public const string Tile22 = "tile-22";
        public const string Tile23 = "tile-23";
        public const string Tile24 = "tile-24";

        // Row 7
        public const string Tile25 = "tile-25";
        public const string Tile26 = "tile-26";
        public const string Tile27 = "tile-27";
        public const string Tile28 = "tile-28";

        // Row 8 (from white perspective, top)
        public const string Tile29 = "tile-29";
        public const string Tile30 = "tile-30";
        public const string Tile31 = "tile-31";
        public const string Tile32 = "tile-32";
    }
}
