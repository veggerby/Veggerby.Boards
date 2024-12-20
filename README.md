# Veggerby.Boards

Veggerby.Boards is a generic framework for board games. E.g.

* Backgammon
* Chess
* Ludo
* Monopoly
* Risc
* Tic-Tac-Toe
* Go (probably/maybe)

Could potentially also be used for games like Kalaha.

## Components

The following sections explain the different components of the framework.

### Concept

A board game consists of 3 pieces:

1. Artifacts - the individual pieces - physical or abstract - that makes up the game setup
2. State - the progress of the game play
3. Flows - the mechanisms - rules - that makes up the game play

So essentially the "what" (artifacts), the "where" (state) and the "how" (flows).

These 3 components are controlled/combined by the GameEngine.

Artifacts are independent of Flows and State. State is defining the state of Artifacts, and
finally Flows control State changes (of Artifacts).

### Artifacts

Artifacts are (semi-) physical entities of the board game, e.g. the board, the pieces, the
dice, any cards or other elements.

Players are also considered game Artifacts, but in a "static" sense, i.e. for Backgammon
there are two players (black and white) - when playing/evaluating the game, a Person can
then "play as e.g. black".

All Artifacts are immutable and statically defined.

Artifacts are built using a GameBuilder.

#### Board and Tiles

Tiles on a Board are also Artifacts (albeit they are not physical entities themselves).
A Board consists of Tiles and Pieces. Tiles have (static) relations to other Tiles, i.e.
Tile B is connected to Tile C in a clockwise direction, but to Tile-A in a counter-clockwise
direction. Relations also have Distance (default to 1), which can be used to specify
relative weight between Relations.

#### Pieces

Pieces are the individual game pieces. They are (again) statically defined, e.g. in Chess
there would be a White King, a White Queen, 2 White Rooks, 2 White Knights, 2 White Bishops
and 8 White Pawns (and similarly for Black), i.e. a total of 32 pieces.

Pieces have movement Patterns assigned, e.g. in Chess a Queen can move (linerarly) unbounded
in any direction, a King can move 1 step in any direction, but a Knight can move in an L
shaped pattern (e.g. 2 steps north and 1 step east).

Patterns are used to evaluate if a given move ("move piece A from tile 1 to tile 2") is valid.
This validity is evaluated via rules and can be subject to additional restrictions, e.g. in
Chess the Queen can move in any (linear) direction as long as no tile is blocked, where as
a knight can "jump over" other occupied tiles. In Backgammon the moves will be restricted by
the valid dice roll.

### Flows

Any game consists of a set of phases, which essentially is a Finite State Machine acting on the
state of the game. Examples of game phases could be:

* A turn
* Initial setup - e.g. placing armies in Risk
* End-game - e.g. moving pieces of the board in Backgammon

### State

A state applies to an artifact.