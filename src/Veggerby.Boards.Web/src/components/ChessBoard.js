import React from 'react';
import * as Black from './svg/chess/Black';
import * as White from './svg/chess/White';

const Tiles = {
    'tile-a1': { x: 0, y: 0 },
    'tile-a2': { x: 0, y: 1 },
    'tile-a3': { x: 0, y: 2 },
    'tile-a4': { x: 0, y: 3 },
    'tile-a5': { x: 0, y: 4 },
    'tile-a6': { x: 0, y: 5 },
    'tile-a7': { x: 0, y: 6 },
    'tile-a8': { x: 0, y: 7 },

    'tile-b1': { x: 1, y: 0 },
    'tile-b2': { x: 1, y: 1 },
    'tile-b3': { x: 1, y: 2 },
    'tile-b4': { x: 1, y: 3 },
    'tile-b5': { x: 1, y: 4 },
    'tile-b6': { x: 1, y: 5 },
    'tile-b7': { x: 1, y: 6 },
    'tile-b8': { x: 1, y: 7 },

    'tile-c1': { x: 2, y: 0 },
    'tile-c2': { x: 2, y: 1 },
    'tile-c3': { x: 2, y: 2 },
    'tile-c4': { x: 2, y: 3 },
    'tile-c5': { x: 2, y: 4 },
    'tile-c6': { x: 2, y: 5 },
    'tile-c7': { x: 2, y: 6 },
    'tile-c8': { x: 2, y: 7 },

    'tile-d1': { x: 3, y: 0 },
    'tile-d2': { x: 3, y: 1 },
    'tile-d3': { x: 3, y: 2 },
    'tile-d4': { x: 3, y: 3 },
    'tile-d5': { x: 3, y: 4 },
    'tile-d6': { x: 3, y: 5 },
    'tile-d7': { x: 3, y: 6 },
    'tile-d8': { x: 3, y: 7 },

    'tile-e1': { x: 4, y: 0 },
    'tile-e2': { x: 4, y: 1 },
    'tile-e3': { x: 4, y: 2 },
    'tile-e4': { x: 4, y: 3 },
    'tile-e5': { x: 4, y: 4 },
    'tile-e6': { x: 4, y: 5 },
    'tile-e7': { x: 4, y: 6 },
    'tile-e8': { x: 4, y: 7 },

    'tile-f1': { x: 5, y: 0 },
    'tile-f2': { x: 5, y: 1 },
    'tile-f3': { x: 5, y: 2 },
    'tile-f4': { x: 5, y: 3 },
    'tile-f5': { x: 5, y: 4 },
    'tile-f6': { x: 5, y: 5 },
    'tile-f7': { x: 5, y: 6 },
    'tile-f8': { x: 5, y: 7 },

    'tile-g1': { x: 6, y: 0 },
    'tile-g2': { x: 6, y: 1 },
    'tile-g3': { x: 6, y: 2 },
    'tile-g4': { x: 6, y: 3 },
    'tile-g5': { x: 6, y: 4 },
    'tile-g6': { x: 6, y: 5 },
    'tile-g7': { x: 6, y: 6 },
    'tile-g8': { x: 6, y: 7 },

    'tile-h1': { x: 7, y: 0 },
    'tile-h2': { x: 7, y: 1 },
    'tile-h3': { x: 7, y: 2 },
    'tile-h4': { x: 7, y: 3 },
    'tile-h5': { x: 7, y: 4 },
    'tile-h6': { x: 7, y: 5 },
    'tile-h7': { x: 7, y: 6 },
    'tile-h8': { x: 7, y: 7 },
};

const Piece = ({ piece }) => {
    switch (piece.pieceId) {
        case 'white-rook-1':
        case 'white-rook-2':
            return <White.Rook />;
        case 'white-knight-1':
        case 'white-knight-2':
            return <White.Knight />;
        case 'white-bishop-1':
        case 'white-bishop-2':
            return <White.Bishop />;
        case 'white-king':
            return <White.King />;
        case 'white-queen':
            return <White.Queen />;
        case 'white-pawn-1':
        case 'white-pawn-2':
        case 'white-pawn-3':
        case 'white-pawn-4':
        case 'white-pawn-5':
        case 'white-pawn-6':
        case 'white-pawn-7':
        case 'white-pawn-8':
            return <White.Pawn />;
        case 'black-rook-1':
        case 'black-rook-2':
            return <Black.Rook />;
        case 'black-knight-1':
        case 'black-knight-2':
            return <Black.Knight />;
        case 'black-bishop-1':
        case 'black-bishop-2':
            return <Black.Bishop />;
        case 'black-king':
            return <Black.King />;
        case 'black-queen':
            return <Black.Queen />;
        case 'black-pawn-1':
        case 'black-pawn-2':
        case 'black-pawn-3':
        case 'black-pawn-4':
        case 'black-pawn-5':
        case 'black-pawn-6':
        case 'black-pawn-7':
        case 'black-pawn-8':
            return <Black.Pawn />;
        default:
            return null;
    }
}

const Tile = ({ tile }) =>
{
    let tilePos = Tiles[tile.tileId];
    let x = tilePos.x * 50;
    let y = tilePos.y * 50;
    let color = (tilePos.x + tilePos.y) % 2 === 1 ? '#aaa' : '#ffffff';

    return <g transform={`translate(${x} ${y})`}>
        <polygon points="0,0 0,50 50,50 50,0" stroke="#000000" fill={color} />
        {tile.pieces.map((piece, ixc) => <Piece key={ixc} piece={piece} />)}
    </g>;
}

const ChessBoard = ({ board }) => board ?
<div>
    <svg
        width={800}
        height={800}
        viewBox={`0 0 ${420} ${420}`}
    >
        <g id="board">
            <polygon
                points="0,0 420,0 420,420, 0,420"
                style={{
                    fill: '#e8e8e8',
                    stroke: 'black',
                    strokeWidth: 1
                }}
            id="border-outer" />
            <g transform="translate(10 10)">
                {board.tiles.map((tile, ixc) => <Tile key={ixc} tile={tile} />)}
            </g>
        </g>
    </svg>
</div>
: null;

export default ChessBoard;