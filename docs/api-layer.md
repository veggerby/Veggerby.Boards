# API Layer

The `Veggerby.Boards.Api` project provides an ASP.NET Core façade over the engine to:

- Instantiate predefined games (Backgammon, Chess)
- Demonstrate event handling and progression
- Serialize structural + state data into transport models

## Controller Flow (`GamesController`)

1. Accepts a GUID id (demo logic: IDs ending in `1` => Backgammon; others => Chess).
2. Builds game via respective `GameBuilder` and calls `Compile()` -> initial `GameProgress`.
3. Optionally simulates events (e.g., rolling dice, moving a piece) to produce a new `GameProgress`.
4. Maps domain objects to DTOs using AutoMapper profiles (see `Models/` namespace).
5. Returns a `GameModel` including board structure, artifact summaries, and state.

## Mapping Models

Models under `Models/` (e.g., `GameModel`, `BoardModel`, `PieceModel`, `DiceModel`, `TileModel`) represent a flattened, serialization-friendly view.

Typical properties:
- IDs and labels
- Piece locations (derived from `PieceState`)
- Dice values (from `DiceState<T>` or null when `NullDiceState`)

## Event Simulation Example

Backgammon branch logic (simplified):
```csharp
progress = progress.HandleEvent(new RollDiceGameEvent<int>(new DiceState<int>(dice1, 3), new DiceState<int>(dice2, 1)));
// Resolve path via visitor on piece pattern, then move
progress = progress.HandleEvent(new MovePieceGameEvent(piece, path));
```

## Extension: Exposing New Games

1. Implement a new `GameBuilder` in a sibling project.
2. Register AutoMapper profiles for any new DTO fields.
3. Add route logic or new endpoints to construct the game and process incoming events.
4. Validate external inputs rigorously (current example uses internal simulation only).

## Suggested Enhancements

- POST endpoint to accept JSON-encoded events (`MovePiece`, `RollDice`).
- Query parameter to control number of simulated steps.
- WebSocket hub for streaming state progression.
- OpenAPI/Swagger documentation for client generation.

## Security & Validation

Current API is demonstrative only:
- No authentication / authorization.
- No user input validation beyond basic parameter parsing.

Before production exposure, add:
- Input schemas & validation
- Rate-limiting / auth
- Exception filters mapping engine errors to 4xx codes

## Serialization Considerations

- Circular references avoided: domain model is acyclic for serialization.
- Prefer explicit DTO projection over exposing engine internals directly.
- Version response contracts if evolving public API.

## Testing the API

Use the provided `.http` file or curl:
```
GET /api/games/{guid}
```
Choose a GUID ending with `1` for Backgammon state demonstration.
