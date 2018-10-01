import * as actions from './actions';

export const fetchGame = (gameId) =>
    (dispatch) => {
        fetch(`/api/games/${gameId}`, {
            method: 'GET',
            headers: { 'Content-Type': 'application/json' }
        })
            .then(result => result.json())
            .then(board => dispatch(actions.fetchGameSuccess(board)))
            .catch(e => dispatch(actions.fetchGameHasErrored(true)));
    };

export const movePiece = (gameId, pieceId, fromTileId, toTileId) =>
    (dispatch) => {
        let body = { pieceId, fromTileId, toTileId };

        fetch(`/api/games/${gameId}/moves`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(body)
        })
            .then(result => result.json())
            .then(board => dispatch(actions.movePieceSuccess(board)))
            .catch(e => dispatch(actions.movePieceHasErrored(true)));
    };