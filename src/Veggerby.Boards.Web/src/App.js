import React, { Component } from 'react';
import { Switch, Route } from "react-router-dom";

import Navigation from './components/Navigation';
import Backgammon from './containers/Backgammon';

class App extends Component {
    render() {
        return (
            <div>
                <Navigation />

                <main role="main" className="container">
                    <Switch>
                        <Route exact path={'/'} component={() => (<strong>Select option in menu...</strong>)} />
                        <Route exact path={'/backgammon'} component={Backgammon} />
                    </Switch>
                </main>
            </div>
        );
    }
}

export default App;
