namespace Veggerby.Boards.Chess.Constants;

/// <summary>
/// Provides constant identifiers for chess tiles (board squares) and players.
/// </summary>
public static class ChessIds
{
    /// <summary>Player identifier constants.</summary>
    public static class Players
    {
        /// <summary>Identifier for the white player.</summary>
        public const string White = "white";
        /// <summary>Identifier for the black player.</summary>
        public const string Black = "black";
    }

    /// <summary>Piece archetype identifier suffixes.</summary>
    public static class PieceSuffixes
    {
        /// <summary>Suffix used in piece identifiers for kings.</summary>
        public const string King = "-king";
        /// <summary>Suffix used in piece identifiers for queens.</summary>
        public const string Queen = "-queen";
        /// <summary>Suffix used in piece identifiers for rooks.</summary>
        public const string Rook = "-rook";
        /// <summary>Suffix used in piece identifiers for bishops.</summary>
        public const string Bishop = "-bishop";
        /// <summary>Suffix used in piece identifiers for knights.</summary>
        public const string Knight = "-knight";
        /// <summary>Suffix used in piece identifiers for pawns.</summary>
        public const string Pawn = "-pawn";
    }

    /// <summary>
    /// Tile identifiers (file + rank) prefixed with "tile-".
    /// </summary>
    public static class Tiles
    {
        /// <summary>Common prefix for tile identifiers.</summary>
        public const string TilePrefix = "tile-";

        // File (column) designators
        /// <summary>File a designator.</summary>
        public const string FileA = "a";
        /// <summary>File b designator.</summary>
        public const string FileB = "b";
        /// <summary>File c designator.</summary>
        public const string FileC = "c";
        /// <summary>File d designator.</summary>
        public const string FileD = "d";
        /// <summary>File e designator.</summary>
        public const string FileE = "e";
        /// <summary>File f designator.</summary>
        public const string FileF = "f";
        /// <summary>File g designator.</summary>
        public const string FileG = "g";
        /// <summary>File h designator.</summary>
        public const string FileH = "h";

        // Rank (row) designators
        /// <summary>Rank 1 designator.</summary>
        public const string Rank1 = "1";
        /// <summary>Rank 2 designator.</summary>
        public const string Rank2 = "2";
        /// <summary>Rank 3 designator.</summary>
        public const string Rank3 = "3";
        /// <summary>Rank 4 designator.</summary>
        public const string Rank4 = "4";
        /// <summary>Rank 5 designator.</summary>
        public const string Rank5 = "5";
        /// <summary>Rank 6 designator.</summary>
        public const string Rank6 = "6";
        /// <summary>Rank 7 designator.</summary>
        public const string Rank7 = "7";
        /// <summary>Rank 8 designator.</summary>
        public const string Rank8 = "8";

        // Generated composite identifiers (readonly to ensure runtime composition clarity; const would inline anyway)
        /// <summary>Square a1.</summary>
        public static readonly string A1 = TilePrefix + FileA + Rank1;
        /// <summary>Square b1.</summary>
        public static readonly string B1 = TilePrefix + FileB + Rank1;
        /// <summary>Square c1.</summary>
        public static readonly string C1 = TilePrefix + FileC + Rank1;
        /// <summary>Square d1.</summary>
        public static readonly string D1 = TilePrefix + FileD + Rank1;
        /// <summary>Square e1.</summary>
        public static readonly string E1 = TilePrefix + FileE + Rank1;
        /// <summary>Square f1.</summary>
        public static readonly string F1 = TilePrefix + FileF + Rank1;
        /// <summary>Square g1.</summary>
        public static readonly string G1 = TilePrefix + FileG + Rank1;
        /// <summary>Square h1.</summary>
        public static readonly string H1 = TilePrefix + FileH + Rank1;
        /// <summary>Square a2.</summary>
        public static readonly string A2 = TilePrefix + FileA + Rank2;
        /// <summary>Square b2.</summary>
        public static readonly string B2 = TilePrefix + FileB + Rank2;
        /// <summary>Square c2.</summary>
        public static readonly string C2 = TilePrefix + FileC + Rank2;
        /// <summary>Square d2.</summary>
        public static readonly string D2 = TilePrefix + FileD + Rank2;
        /// <summary>Square e2.</summary>
        public static readonly string E2 = TilePrefix + FileE + Rank2;
        /// <summary>Square f2.</summary>
        public static readonly string F2 = TilePrefix + FileF + Rank2;
        /// <summary>Square g2.</summary>
        public static readonly string G2 = TilePrefix + FileG + Rank2;
        /// <summary>Square h2.</summary>
        public static readonly string H2 = TilePrefix + FileH + Rank2;
        /// <summary>Square a3.</summary>
        public static readonly string A3 = TilePrefix + FileA + Rank3;
        /// <summary>Square b3.</summary>
        public static readonly string B3 = TilePrefix + FileB + Rank3;
        /// <summary>Square c3.</summary>
        public static readonly string C3 = TilePrefix + FileC + Rank3;
        /// <summary>Square d3.</summary>
        public static readonly string D3 = TilePrefix + FileD + Rank3;
        /// <summary>Square e3.</summary>
        public static readonly string E3 = TilePrefix + FileE + Rank3;
        /// <summary>Square f3.</summary>
        public static readonly string F3 = TilePrefix + FileF + Rank3;
        /// <summary>Square g3.</summary>
        public static readonly string G3 = TilePrefix + FileG + Rank3;
        /// <summary>Square h3.</summary>
        public static readonly string H3 = TilePrefix + FileH + Rank3;
        /// <summary>Square a4.</summary>
        public static readonly string A4 = TilePrefix + FileA + Rank4;
        /// <summary>Square b4.</summary>
        public static readonly string B4 = TilePrefix + FileB + Rank4;
        /// <summary>Square c4.</summary>
        public static readonly string C4 = TilePrefix + FileC + Rank4;
        /// <summary>Square d4.</summary>
        public static readonly string D4 = TilePrefix + FileD + Rank4;
        /// <summary>Square e4.</summary>
        public static readonly string E4 = TilePrefix + FileE + Rank4;
        /// <summary>Square f4.</summary>
        public static readonly string F4 = TilePrefix + FileF + Rank4;
        /// <summary>Square g4.</summary>
        public static readonly string G4 = TilePrefix + FileG + Rank4;
        /// <summary>Square h4.</summary>
        public static readonly string H4 = TilePrefix + FileH + Rank4;
        /// <summary>Square a5.</summary>
        public static readonly string A5 = TilePrefix + FileA + Rank5;
        /// <summary>Square b5.</summary>
        public static readonly string B5 = TilePrefix + FileB + Rank5;
        /// <summary>Square c5.</summary>
        public static readonly string C5 = TilePrefix + FileC + Rank5;
        /// <summary>Square d5.</summary>
        public static readonly string D5 = TilePrefix + FileD + Rank5;
        /// <summary>Square e5.</summary>
        public static readonly string E5 = TilePrefix + FileE + Rank5;
        /// <summary>Square f5.</summary>
        public static readonly string F5 = TilePrefix + FileF + Rank5;
        /// <summary>Square g5.</summary>
        public static readonly string G5 = TilePrefix + FileG + Rank5;
        /// <summary>Square h5.</summary>
        public static readonly string H5 = TilePrefix + FileH + Rank5;
        /// <summary>Square a6.</summary>
        public static readonly string A6 = TilePrefix + FileA + Rank6;
        /// <summary>Square b6.</summary>
        public static readonly string B6 = TilePrefix + FileB + Rank6;
        /// <summary>Square c6.</summary>
        public static readonly string C6 = TilePrefix + FileC + Rank6;
        /// <summary>Square d6.</summary>
        public static readonly string D6 = TilePrefix + FileD + Rank6;
        /// <summary>Square e6.</summary>
        public static readonly string E6 = TilePrefix + FileE + Rank6;
        /// <summary>Square f6.</summary>
        public static readonly string F6 = TilePrefix + FileF + Rank6;
        /// <summary>Square g6.</summary>
        public static readonly string G6 = TilePrefix + FileG + Rank6;
        /// <summary>Square h6.</summary>
        public static readonly string H6 = TilePrefix + FileH + Rank6;
        /// <summary>Square a7.</summary>
        public static readonly string A7 = TilePrefix + FileA + Rank7;
        /// <summary>Square b7.</summary>
        public static readonly string B7 = TilePrefix + FileB + Rank7;
        /// <summary>Square c7.</summary>
        public static readonly string C7 = TilePrefix + FileC + Rank7;
        /// <summary>Square d7.</summary>
        public static readonly string D7 = TilePrefix + FileD + Rank7;
        /// <summary>Square e7.</summary>
        public static readonly string E7 = TilePrefix + FileE + Rank7;
        /// <summary>Square f7.</summary>
        public static readonly string F7 = TilePrefix + FileF + Rank7;
        /// <summary>Square g7.</summary>
        public static readonly string G7 = TilePrefix + FileG + Rank7;
        /// <summary>Square h7.</summary>
        public static readonly string H7 = TilePrefix + FileH + Rank7;
        /// <summary>Square a8.</summary>
        public static readonly string A8 = TilePrefix + FileA + Rank8;
        /// <summary>Square b8.</summary>
        public static readonly string B8 = TilePrefix + FileB + Rank8;
        /// <summary>Square c8.</summary>
        public static readonly string C8 = TilePrefix + FileC + Rank8;
        /// <summary>Square d8.</summary>
        public static readonly string D8 = TilePrefix + FileD + Rank8;
        /// <summary>Square e8.</summary>
        public static readonly string E8 = TilePrefix + FileE + Rank8;
        /// <summary>Square f8.</summary>
        public static readonly string F8 = TilePrefix + FileF + Rank8;
        /// <summary>Square g8.</summary>
        public static readonly string G8 = TilePrefix + FileG + Rank8;
        /// <summary>Square h8.</summary>
        public static readonly string H8 = TilePrefix + FileH + Rank8;
    }

    /// <summary>Canonical piece identifiers for initial set and indexed variants.</summary>
    public static class Pieces
    {
        // White major pieces
        /// <summary>White king piece id.</summary>
        public const string WhiteKing = "white-king";
        /// <summary>White queen piece id.</summary>
        public const string WhiteQueen = "white-queen";
        /// <summary>White rook queenside (index 1).</summary>
        public const string WhiteRook1 = "white-rook-1";
        /// <summary>White rook kingside (index 2).</summary>
        public const string WhiteRook2 = "white-rook-2";
        /// <summary>White bishop queenside (index 1).</summary>
        public const string WhiteBishop1 = "white-bishop-1";
        /// <summary>White bishop kingside (index 2).</summary>
        public const string WhiteBishop2 = "white-bishop-2";
        /// <summary>White knight queenside (index 1).</summary>
        public const string WhiteKnight1 = "white-knight-1";
        /// <summary>White knight kingside (index 2).</summary>
        public const string WhiteKnight2 = "white-knight-2";

        // Black major pieces
        /// <summary>Black king piece id.</summary>
        public const string BlackKing = "black-king";
        /// <summary>Black queen piece id.</summary>
        public const string BlackQueen = "black-queen";
        /// <summary>Black rook queenside (index 1).</summary>
        public const string BlackRook1 = "black-rook-1";
        /// <summary>Black rook kingside (index 2).</summary>
        public const string BlackRook2 = "black-rook-2";
        /// <summary>Black bishop queenside (index 1).</summary>
        public const string BlackBishop1 = "black-bishop-1";
        /// <summary>Black bishop kingside (index 2).</summary>
        public const string BlackBishop2 = "black-bishop-2";
        /// <summary>Black knight queenside (index 1).</summary>
        public const string BlackKnight1 = "black-knight-1";
        /// <summary>Black knight kingside (index 2).</summary>
        public const string BlackKnight2 = "black-knight-2";

        // White pawns (file indexed a-h => 1-8)
        /// <summary>White pawn on a-file (index 1).</summary>
        public const string WhitePawn1 = "white-pawn-1";
        /// <summary>White pawn on b-file (index 2).</summary>
        public const string WhitePawn2 = "white-pawn-2";
        /// <summary>White pawn on c-file (index 3).</summary>
        public const string WhitePawn3 = "white-pawn-3";
        /// <summary>White pawn on d-file (index 4).</summary>
        public const string WhitePawn4 = "white-pawn-4";
        /// <summary>White pawn on e-file (index 5).</summary>
        public const string WhitePawn5 = "white-pawn-5";
        /// <summary>White pawn on f-file (index 6).</summary>
        public const string WhitePawn6 = "white-pawn-6";
        /// <summary>White pawn on g-file (index 7).</summary>
        public const string WhitePawn7 = "white-pawn-7";
        /// <summary>White pawn on h-file (index 8).</summary>
        public const string WhitePawn8 = "white-pawn-8";

        // Black pawns
        /// <summary>Black pawn on a-file (index 1).</summary>
        public const string BlackPawn1 = "black-pawn-1";
        /// <summary>Black pawn on b-file (index 2).</summary>
        public const string BlackPawn2 = "black-pawn-2";
        /// <summary>Black pawn on c-file (index 3).</summary>
        public const string BlackPawn3 = "black-pawn-3";
        /// <summary>Black pawn on d-file (index 4).</summary>
        public const string BlackPawn4 = "black-pawn-4";
        /// <summary>Black pawn on e-file (index 5).</summary>
        public const string BlackPawn5 = "black-pawn-5";
        /// <summary>Black pawn on f-file (index 6).</summary>
        public const string BlackPawn6 = "black-pawn-6";
        /// <summary>Black pawn on g-file (index 7).</summary>
        public const string BlackPawn7 = "black-pawn-7";
        /// <summary>Black pawn on h-file (index 8).</summary>
        public const string BlackPawn8 = "black-pawn-8";
    }
}