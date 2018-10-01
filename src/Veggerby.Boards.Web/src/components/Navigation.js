import React from 'react';
import Navbar from 'react-bootstrap/lib/Navbar';
import Nav from 'react-bootstrap/lib/Nav';
import NavItem from 'react-bootstrap/lib/NavItem';
import NavDropdown from 'react-bootstrap/lib/NavDropdown';
import MenuItem from 'react-bootstrap/lib/MenuItem';

const Navigation = () =>
(
    <Navbar>
        <Navbar.Header>
            <Navbar.Brand>
                <a href="/">Veggerby Boards</a>
            </Navbar.Brand>
        </Navbar.Header>
        <Nav>
            <NavItem eventKey={1} href="/">
                Home
            </NavItem>
            <NavDropdown eventKey={2} title="Games" id="basic-nav-dropdown">
                <MenuItem eventKey={2.1} href="/backgammon">Backgammon</MenuItem>
            </NavDropdown>
        </Nav>
    </Navbar>
);

export default Navigation;