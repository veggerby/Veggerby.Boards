import React from 'react';

const stroke = 2;
const width = 800;
const margin = 30;
const barWidth = 50;
const rowSpacing = 100;
const tileWidth = (width - (2 * margin) - barWidth) / 12;
const tileHeight = 5 * tileWidth;// (height - (2 * margin) - rowSpacing) / 2;
const height = 2 * (margin + tileHeight) + rowSpacing;

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
        transform={`translate(${Math.floor(number / 5) * 5}, ${getDirection(tile) * ((number % 5) + (getDirection(tile) < 0 ? 1 : 0)) * (tileWidth + stroke) + Math.floor(number / 5) * 5})`}
    >
        <circle
            cx={tileWidth / 2}
            cy={tileWidth / 2}
            r={tileWidth / 2}
            style={{
                'stroke': piece.ownerId === 'white' ? '#000' : '#555',
                'stroke-width': stroke,
                'fill': piece.ownerId === 'white' ? '#fff' : '#000'
            }}/>
        <text
            x={tileWidth / 2}
            y={tileWidth / 2}
            style={{
                'font-size': 8,
                'text-anchor': 'middle',
                'alignment-baseline': 'middle',
                'fill': piece.ownerId === 'black' ? '#fff' : '#000'
            }}>{piece.pieceId}</text>
    </g>;

const Tile = ({ tile  }) => {
    return <g
        transform={`translate(${getX(tile)} ${getY(tile)})`}
    >
        <polygon
            points={`0,0 ${tileWidth/2},${getDirection(tile) * tileHeight} ${tileWidth},0`}
            style={{
                'fill': (getTileIxc(tile) % 2 === 0 ? '#ddd' : '#aaa'),
                'stroke': 'black',
                'stroke-width': stroke
            }}
            id={tile.tileId} />
        {tile.pieces.map((piece, ixc) => <Piece key={ixc} piece={piece} tile={tile} number={ixc} />)}
        <text
            x={tileWidth / 2}
            y={-1 * getDirection(tile) * 10}
            style={{
                'font-size': 8,
                'text-anchor': 'middle',
                'alignment-baseline': 'middle'
            }}>{tile.tileId}</text>
    </g>;
};

const BackgammonBoard = ({ board }) => board ?
<div>
    <svg width={width} height={height}>
        <g
            id="board"
        >
        <polygon
            points={`${stroke / 2},${stroke / 2} ${width - stroke / 2},${stroke / 2} ${width - stroke / 2},${height - stroke / 2} ${stroke / 2},${height  - stroke / 2}`}
            style={{
                'fill': '#e8e8e8',
                'stroke': 'black',
                'stroke-width': stroke
            }}
        id="border-outer" />
        <polygon
            points={`${margin},${margin} ${(width - barWidth) / 2},${margin} ${(width - barWidth) / 2},${height - margin} ${margin},${height - margin}`}
            style={{
                'fill': '#ccc',
                'stroke': 'black',
                'stroke-width': stroke
            }}
            id="left-half"/>
        <polygon
            points={`${width - margin},${margin} ${(width + barWidth) / 2},${margin} ${(width + barWidth) / 2},${height - margin} ${width - margin},${height - margin}`}
            style={{
                'fill': '#ccc',
                'stroke': 'black',
                'stroke-width': stroke
            }}
            id="right-half"/>
            {board.tiles.filter(x => x.tileId.startsWith('point-')).map((tile, ixc) => <Tile key={ixc} tile={tile} />)}
        </g>
    </svg>
</div>
: null;

export default BackgammonBoard;