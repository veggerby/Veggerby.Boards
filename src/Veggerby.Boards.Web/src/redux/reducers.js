import * as actions from "./actions";

export const game = (state = null, action) => {
    switch (action.type) {
        case actions.FETCH_GAME_SUCCESS:
            console.log('FETCH GAME SUCCESS', action.game);
            return action.game;
        case actions.FETCH_GAME_HAS_ERRORED:
            console.log('FETCH GAME ERROR', action.hasErrored);
            return state;
        case actions.MOVE_PIECE_SUCCESS:
            console.log('MOVE PIECE SUCCESS', action.game);
            return action.game;
        case actions.MOVE_PIECE_HAS_ERRORED:
            console.log('MOVE PIECE ERROR', action.hasErrored);
            return state;
        default:
            return state;
    }
}
