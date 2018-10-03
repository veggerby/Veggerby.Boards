import React from 'react';
import * as Black from './svg/chess/Black';
import * as White from './svg/chess/White';

const ChessBoard = ({ board }) => board ?
<div>
    <svg
        width={600}
        height={600}
        viewBox={`0 0 ${600} ${600}`}
    >
        <g id="board">
            <polygon
                points="0,0 600,0 600,600, 0,600"
                style={{
                    fill: '#e8e8e8',
                    stroke: 'black',
                    strokeWidth: 1
                }}
            id="border-outer" />
            <g>
                <g transform="translate(0 0)">
                    <Black.Rook />
                </g>
                <g transform="translate(50 0)">
                    <Black.Knight />
                </g>
                <g transform="translate(100 0)">
                    <Black.Bishop />
                </g>
                <g transform="translate(150 0)">
                    <Black.King />
                </g>
                <g transform="translate(200 0)">
                    <Black.Queen />
                </g>
                <g transform="translate(250 0)">
                    <Black.Bishop />
                </g>
                <g transform="translate(300 0)">
                    <Black.Knight />
                </g>
                <g transform="translate(350 0)">
                    <Black.Rook />
                </g>

                <g transform="translate(0 50)">
                    <Black.Pawn />
                </g>
                <g transform="translate(50 50)">
                    <Black.Pawn />
                </g>
                <g transform="translate(100 50)">
                    <Black.Pawn />
                </g>
                <g transform="translate(150 50)">
                    <Black.Pawn />
                </g>
                <g transform="translate(200 50)">
                    <Black.Pawn />
                </g>
                <g transform="translate(250 50)">
                    <Black.Pawn />
                </g>
                <g transform="translate(300 50)">
                    <Black.Pawn />
                </g>
                <g transform="translate(350 50)">
                    <Black.Pawn />
                </g>
            </g>

            <g transform="translate(0 100)">
                <g transform="translate(0 0)">
                    <White.Rook />
                </g>
                <g transform="translate(50 0)">
                    <White.Knight />
                </g>
                <g transform="translate(100 0)">
                    <White.Bishop />
                </g>
                <g transform="translate(150 0)">
                    <White.King />
                </g>
                <g transform="translate(200 0)">
                    <White.Queen />
                </g>
                <g transform="translate(250 0)">
                    <White.Bishop />
                </g>
                <g transform="translate(300 0)">
                    <White.Knight />
                </g>
                <g transform="translate(350 0)">
                    <White.Rook />
                </g>

                <g transform="translate(0 50)">
                    <White.Pawn />
                </g>
                <g transform="translate(50 50)">
                    <White.Pawn />
                </g>
                <g transform="translate(100 50)">
                    <White.Pawn />
                </g>
                <g transform="translate(150 50)">
                    <White.Pawn />
                </g>
                <g transform="translate(200 50)">
                    <White.Pawn />
                </g>
                <g transform="translate(250 50)">
                    <White.Pawn />
                </g>
                <g transform="translate(300 50)">
                    <White.Pawn />
                </g>
                <g transform="translate(350 50)">
                    <White.Pawn />
                </g>
            </g>
        </g>
    </svg>
</div>
: null;

export default ChessBoard;