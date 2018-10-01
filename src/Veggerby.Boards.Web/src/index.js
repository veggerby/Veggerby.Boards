import React from 'react';
import ReactDOM from 'react-dom';

import registerServiceWorker from './registerServiceWorker';

import { createStore, combineReducers, applyMiddleware, compose } from "redux";
import { Provider } from 'react-redux';
import { ConnectedRouter, connectRouter, routerMiddleware } from 'connected-react-router'
import thunk from 'redux-thunk';

import { createBrowserHistory } from 'history';

import * as reducers from "./redux/reducers";

import App from './App';

const history = createBrowserHistory();

const composeEnhancers = window.__REDUX_DEVTOOLS_EXTENSION_COMPOSE__ || compose;

const middleware = [
    routerMiddleware(history),
    thunk
];

const store = createStore(
    connectRouter(history)(
        combineReducers({
            ...reducers
        })
    ),
    composeEnhancers(
        applyMiddleware(...middleware))
);

ReactDOM.render(
    <Provider store={store}>
        <ConnectedRouter history={history}>
            <App />
        </ConnectedRouter>
    </Provider>,
    document.getElementById('root')
);

registerServiceWorker();
