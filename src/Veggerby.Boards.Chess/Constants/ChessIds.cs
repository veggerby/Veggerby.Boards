using System;

namespace Veggerby.Boards.Chess;

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
        // Generated constants for all 64 squares a1..h8.
        /// <summary>Square a1.</summary>
        public const string A1 = "tile-a1";
        /// <summary>Square b1.</summary>
        public const string B1 = "tile-b1";
        /// <summary>Square c1.</summary>
        public const string C1 = "tile-c1";
        /// <summary>Square d1.</summary>
        public const string D1 = "tile-d1";
        /// <summary>Square e1.</summary>
        public const string E1 = "tile-e1";
        /// <summary>Square f1.</summary>
        public const string F1 = "tile-f1";
        /// <summary>Square g1.</summary>
        public const string G1 = "tile-g1";
        /// <summary>Square h1.</summary>
        public const string H1 = "tile-h1";
        /// <summary>Square a2.</summary>
        public const string A2 = "tile-a2";
        /// <summary>Square b2.</summary>
        public const string B2 = "tile-b2";
        /// <summary>Square c2.</summary>
        public const string C2 = "tile-c2";
        /// <summary>Square d2.</summary>
        public const string D2 = "tile-d2";
        /// <summary>Square e2.</summary>
        public const string E2 = "tile-e2";
        /// <summary>Square f2.</summary>
        public const string F2 = "tile-f2";
        /// <summary>Square g2.</summary>
        public const string G2 = "tile-g2";
        /// <summary>Square h2.</summary>
        public const string H2 = "tile-h2";
        /// <summary>Square a3.</summary>
        public const string A3 = "tile-a3";
        /// <summary>Square b3.</summary>
        public const string B3 = "tile-b3";
        /// <summary>Square c3.</summary>
        public const string C3 = "tile-c3";
        /// <summary>Square d3.</summary>
        public const string D3 = "tile-d3";
        /// <summary>Square e3.</summary>
        public const string E3 = "tile-e3";
        /// <summary>Square f3.</summary>
        public const string F3 = "tile-f3";
        /// <summary>Square g3.</summary>
        public const string G3 = "tile-g3";
        /// <summary>Square h3.</summary>
        public const string H3 = "tile-h3";
        /// <summary>Square a4.</summary>
        public const string A4 = "tile-a4";
        /// <summary>Square b4.</summary>
        public const string B4 = "tile-b4";
        /// <summary>Square c4.</summary>
        public const string C4 = "tile-c4";
        /// <summary>Square d4.</summary>
        public const string D4 = "tile-d4";
        /// <summary>Square e4.</summary>
        public const string E4 = "tile-e4";
        /// <summary>Square f4.</summary>
        public const string F4 = "tile-f4";
        /// <summary>Square g4.</summary>
        public const string G4 = "tile-g4";
        /// <summary>Square h4.</summary>
        public const string H4 = "tile-h4";
        /// <summary>Square a5.</summary>
        public const string A5 = "tile-a5";
        /// <summary>Square b5.</summary>
        public const string B5 = "tile-b5";
        /// <summary>Square c5.</summary>
        public const string C5 = "tile-c5";
        /// <summary>Square d5.</summary>
        public const string D5 = "tile-d5";
        /// <summary>Square e5.</summary>
        public const string E5 = "tile-e5";
        /// <summary>Square f5.</summary>
        public const string F5 = "tile-f5";
        /// <summary>Square g5.</summary>
        public const string G5 = "tile-g5";
        /// <summary>Square h5.</summary>
        public const string H5 = "tile-h5";
        /// <summary>Square a6.</summary>
        public const string A6 = "tile-a6";
        /// <summary>Square b6.</summary>
        public const string B6 = "tile-b6";
        /// <summary>Square c6.</summary>
        public const string C6 = "tile-c6";
        /// <summary>Square d6.</summary>
        public const string D6 = "tile-d6";
        /// <summary>Square e6.</summary>
        public const string E6 = "tile-e6";
        /// <summary>Square f6.</summary>
        public const string F6 = "tile-f6";
        /// <summary>Square g6.</summary>
        public const string G6 = "tile-g6";
        /// <summary>Square h6.</summary>
        public const string H6 = "tile-h6";
        /// <summary>Square a7.</summary>
        public const string A7 = "tile-a7";
        /// <summary>Square b7.</summary>
        public const string B7 = "tile-b7";
        /// <summary>Square c7.</summary>
        public const string C7 = "tile-c7";
        /// <summary>Square d7.</summary>
        public const string D7 = "tile-d7";
        /// <summary>Square e7.</summary>
        public const string E7 = "tile-e7";
        /// <summary>Square f7.</summary>
        public const string F7 = "tile-f7";
        /// <summary>Square g7.</summary>
        public const string G7 = "tile-g7";
        /// <summary>Square h7.</summary>
        public const string H7 = "tile-h7";
        /// <summary>Square a8.</summary>
        public const string A8 = "tile-a8";
        /// <summary>Square b8.</summary>
        public const string B8 = "tile-b8";
        /// <summary>Square c8.</summary>
        public const string C8 = "tile-c8";
        /// <summary>Square d8.</summary>
        public const string D8 = "tile-d8";
        /// <summary>Square e8.</summary>
        public const string E8 = "tile-e8";
        /// <summary>Square f8.</summary>
        public const string F8 = "tile-f8";
        /// <summary>Square g8.</summary>
        public const string G8 = "tile-g8";
        /// <summary>Square h8.</summary>
        public const string H8 = "tile-h8";
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

        // Test / scenario specific pieces
        /// <summary>White test pawn (special scenario id).</summary>
        public const string WhitePawnTest = "white-pawn-test";
        /// <summary>Black test pawn (special scenario id).</summary>
        public const string BlackPawnTest = "black-pawn-test";
        /// <summary>Black bishop test blocker (scenario id).</summary>
        public const string BlackBishopBlocker = "black-bishop-blocker";
        /// <summary>Black knight test blocker (scenario id).</summary>
        public const string BlackKnightBlocker = "black-knight-blocker";
    }
}