import React, { Component } from 'react';
import { fetchGame } from '../redux/thunk';
import { bindActionCreators } from 'redux';
import { connect } from 'react-redux';
import * as Actions from '../redux/actions';
import ChessBoard from '../components/ChessBoard';

class Chess extends Component {
    componentDidMount() {
        this.props.fetchGame('00000000-0000-0000-0000-000000000000');
    }

    render() {
        return this.props.game ? <ChessBoard board={this.props.game.board} /> : null;
    }
}

/**
* Map the state to props.
*/
const mapStateToProps = (state) => ({
    game: state.game
});

/**
* Map the actions to props.
*/
const mapDispatchToProps = (dispatch) => ({
    fetchGame: gameId => dispatch(fetchGame(gameId)),
    actions: bindActionCreators(Actions, dispatch)
});

export default connect(
    mapStateToProps,
    mapDispatchToProps
)(Chess);