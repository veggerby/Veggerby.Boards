import React from 'react';

const stroke = 0.75;
const margin = 10;
const barWidth = 15;
const tileWidth = 20;
const pieceRadius = 8;
const tileHeight = 80;
const fontSize = 3;

const width = 2 * margin + 12 * tileWidth + barWidth;
const height = 2 * margin + 2 * tileHeight + 30;

const getTileIxc = tile => tile.tileId.replace('point-', '') - 1;

const getX = tile => {
    let ixc = getTileIxc(tile);
    let x = ixc < 12
        ? width - margin - (ixc + 1) * tileWidth - (ixc > 5 ? barWidth : 0)
        : margin + (ixc - 12) * tileWidth + (ixc > 17 ? barWidth : 0);
    return x;
}

const getY = tile => {
    let ixc = getTileIxc(tile);
    let y = ixc < 12 ? height - margin : margin;
    return y;
}

const getDirection = tile => {
    let ixc = getTileIxc(tile);
    let direction = ixc < 12 ? -1 : 1;
    return direction;
}

const Piece = ({ tile, piece, number }) =>
    <g
        id={piece.pieceId}
        transform={`translate(${Math.floor(number / 5) * (pieceRadius / 5)}, ${getDirection(tile) * ((number % 5) + (getDirection(tile) < 0 ? 1 : 0)) * (2 * pieceRadius + stroke) + Math.floor(number / 5) * (pieceRadius / 5)})`}
    >
        <circle
            cx={tileWidth / 2}
            cy={pieceRadius}
            r={pieceRadius}
            style={{
                stroke: piece.ownerId === 'white' ? '#000' : '#555',
                strokeWidth: stroke,
                fill: piece.ownerId === 'white' ? '#fff' : '#000'
            }}/>
        <text
            x={tileWidth / 2}
            y={pieceRadius}
            style={{
                fontSize: fontSize,
                textAnchor: 'middle',
                alignmentBaseline: 'middle',
                fill: piece.ownerId === 'black' ? '#fff' : '#000'
            }}>{piece.pieceId}</text>
    </g>;

const Tile = ({ tile  }) => {
    return <g
        transform={`translate(${getX(tile)} ${getY(tile)})`}
    >
        <polygon
            points={`0,0 ${tileWidth/2},${getDirection(tile) * tileHeight} ${tileWidth},0`}
            style={{
                fill: (getTileIxc(tile) % 2 === 0 ? '#ddd' : '#aaa'),
                stroke: 'black',
                strokeWidth: stroke
            }}
            id={tile.tileId} />
        {tile.pieces.map((piece, ixc) => <Piece key={ixc} piece={piece} tile={tile} number={ixc} />)}
        <text
            x={tileWidth / 2}
            y={-1 * getDirection(tile) * margin / 2}
            style={{
                fontSize: fontSize,
                textAnchor: 'middle',
                alignmentBaseline: 'middle'
            }}>{tile.tileId}</text>
    </g>;
};

const BackgammonBoard = ({ board }) => board ?
<div>
    <svg
        width={800}
        height={600}
        viewBox={`0 0 ${width} ${height}`}
    >
        <g
            id="board"
        >
            <polygon
                points={`${stroke / 2},${stroke / 2} ${width - stroke / 2},${stroke / 2} ${width - stroke / 2},${height - stroke / 2} ${stroke / 2},${height  - stroke / 2}`}
                style={{
                    fill: '#e8e8e8',
                    stroke: 'black',
                    strokeWidth: stroke
                }}
            id="border-outer" />
            <polygon
                points={`${margin},${margin} ${(width - barWidth) / 2},${margin} ${(width - barWidth) / 2},${height - margin} ${margin},${height - margin}`}
                style={{
                    fill: '#ccc',
                    stroke: 'black',
                    strokeWidth: stroke
                }}
                id="left-half"/>
            <polygon
                points={`${width - margin},${margin} ${(width + barWidth) / 2},${margin} ${(width + barWidth) / 2},${height - margin} ${width - margin},${height - margin}`}
                style={{
                    fill: '#ccc',
                    stroke: 'black',
                    strokeWidth: stroke
                }}
                id="right-half"/>
            {board.tiles.filter(x => x.tileId.startsWith('point-')).map((tile, ixc) => <Tile key={ixc} tile={tile} />)}
        </g>
    </svg>
</div>
: null;

export default BackgammonBoard;