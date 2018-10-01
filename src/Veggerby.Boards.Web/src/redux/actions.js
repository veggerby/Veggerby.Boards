export const FETCH_GAME_SUCCESS = 'FETCH_GAME_SUCCESS';
export const FETCH_GAME_HAS_ERRORED = 'FETCH_GAME_HAS_ERRORED';

export const fetchGameSuccess = board => ({ type: FETCH_GAME_SUCCESS, board });
export const fetchGameHasErrored = hasErrored => ({ type: FETCH_GAME_HAS_ERRORED, hasErrored });

export const MOVE_PIECE_SUCCESS = 'MOVE_PIECE_SUCCESS';
export const MOVE_PIECE_HAS_ERRORED = 'MOVE_PIECE_HAS_ERRORED';

export const movePieceSuccess = board => ({ type: MOVE_PIECE_SUCCESS, board });
export const movePieceHasErrored = hasErrored => ({ type: MOVE_PIECE_HAS_ERRORED, hasErrored });
