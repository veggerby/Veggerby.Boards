# Save/Load and Replay Format

**Status:** Phase 1 Complete (Core Infrastructure)  
**Version:** 1.0  
**Last Updated:** 2026-01-07

## Overview

The Veggerby.Boards replay format provides a stable, versioned, JSON-based serialization format for `GameState` and event history, enabling:

- **Game Persistence**: Save/load games mid-play
- **Deterministic Replay**: Reproduce exact game sequences
- **Game Sharing**: Export/import games for analysis
- **Bug Reports**: Attach reproducible failing game states
- **Analysis Tools**: Load historical games for study
- **Testing**: Generate test cases from real gameplay
- **Audit Trails**: Tournament game verification

## Format Specification

### File Format

Replay files use the `.replay.json` extension and are encoded as UTF-8 JSON.

### Envelope Structure

```json
{
  "format": "veggerby-boards-replay",
  "version": "1.0",
  "metadata": {
    "game": "chess",
    "players": ["player1", "player2"],
    "created": "2026-01-07T12:00:00Z",
    "title": "Example Game",
    "tags": ["test", "example"],
    "customMetadata": {
      "tournament": "Example Tournament",
      "round": "1"
    }
  },
  "initialState": {
    "hash": "1234ABCD5678EF90",
    "hash128": "1234ABCD5678EF901234ABCD5678EF90",
    "artifacts": {
      "piece-1": {
        "Type": "PieceState",
        "ArtifactId": "piece-1",
        "ArtifactType": "Piece",
        "CurrentTile": "tile-a1"
      }
    },
    "turn": {
      "player": "player1",
      "number": 1,
      "segment": "Main"
    },
    "random": {
      "seed": 12345,
      "typeName": "XorShiftRandomSource"
    }
  },
  "events": [
    {
      "index": 0,
      "type": "MovePieceGameEvent",
      "data": {
        "PieceId": "piece-1",
        "FromTileId": "tile-a1",
        "ToTileId": "tile-a2",
        "PathTileIds": ["tile-a1", "tile-a2"]
      },
      "timestamp": "2026-01-07T12:01:00Z",
      "resultHash": "5678ABCD1234EF90",
      "resultHash128": "5678ABCD1234EF905678ABCD1234EF90"
    }
  ],
  "finalState": {
    "hash": "5678ABCD1234EF90",
    "hash128": "5678ABCD1234EF905678ABCD1234EF90",
    "artifacts": {
      "piece-1": {
        "Type": "PieceState",
        "ArtifactId": "piece-1",
        "ArtifactType": "Piece",
        "CurrentTile": "tile-a2"
      }
    }
  }
}
```

## Core API

### IGameReplaySerializer Interface

The primary interface for serialization and validation:

```csharp
public interface IGameReplaySerializer
{
    /// <summary>
    /// Serializes the current game progress to a replay envelope.
    /// </summary>
    ReplayEnvelope Serialize(GameProgress progress);

    /// <summary>
    /// Deserializes a replay envelope back to game progress.
    /// </summary>
    GameProgress Deserialize(ReplayEnvelope envelope);

    /// <summary>
    /// Validates a replay envelope for structural integrity.
    /// </summary>
    ValidationResult Validate(ReplayEnvelope envelope);
}
```

### JsonReplaySerializer Implementation

JSON-based implementation for human-readable replay files:

```csharp
// Create serializer with game context
var serializer = new JsonReplaySerializer(progress.Game, "chess");

// Serialize current progress
var envelope = serializer.Serialize(progress);

// Validate envelope structure
var validation = serializer.Validate(envelope);
if (!validation.IsValid)
{
    throw new InvalidOperationException($"Invalid replay: {string.Join(", ", validation.Errors)}");
}

// Deserialize (future implementation)
// var loadedProgress = serializer.Deserialize(envelope);
```

## Usage Examples

### Basic Save/Load

```csharp
using System.IO;
using System.Text.Json;
using Veggerby.Boards;
using Veggerby.Boards.Serialization;

// Build and play a game
var builder = new ChessGameBuilder();
var progress = builder.Compile();

// Make some moves
progress = progress.HandleEvent(new MovePieceGameEvent(/* ... */));
progress = progress.HandleEvent(new MovePieceGameEvent(/* ... */));

// Serialize to JSON
var serializer = new JsonReplaySerializer(progress.Game, "chess");
var envelope = serializer.Serialize(progress);

// Write to file
var json = JsonSerializer.Serialize(envelope, new JsonSerializerOptions 
{ 
    WriteIndented = true 
});
File.WriteAllText("my-game.replay.json", json);

// Read from file
var loadedJson = File.ReadAllText("my-game.replay.json");
var loadedEnvelope = JsonSerializer.Deserialize<ReplayEnvelope>(loadedJson);

// Validate
var validation = serializer.Validate(loadedEnvelope);
if (!validation.IsValid)
{
    Console.WriteLine($"Validation failed: {string.Join(", ", validation.Errors)}");
}
```

### Creating Famous Game Replays

```csharp
// Example: The Immortal Game (Anderssen vs Kieseritzky, 1851)
var builder = new ChessGameBuilder();
var progress = builder.Compile();

// Apply moves from PGN
// e4 e5, f4 exf4, Bc4 Qh4+, Kf1 b5, ...
foreach (var move in pgn.Moves)
{
    var gameEvent = ConvertPgnToEvent(move);
    progress = progress.HandleEvent(gameEvent);
}

// Serialize with metadata
var serializer = new JsonReplaySerializer(progress.Game, "chess");
var envelope = serializer.Serialize(progress) with
{
    Metadata = new ReplayMetadata
    {
        GameType = "chess",
        Players = new[] { "Adolf Anderssen", "Lionel Kieseritzky" },
        Created = new DateTime(1851, 6, 21),
        Title = "The Immortal Game",
        Tags = new[] { "famous", "romantic-era", "sacrifice", "checkmate" }
    }
};

var json = JsonSerializer.Serialize(envelope, new JsonSerializerOptions { WriteIndented = true });
File.WriteAllText("immortal-game.replay.json", json);
```

## Data Model

### ReplayEnvelope

Main container for serialized game data.

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `format` | string | Yes | Always `"veggerby-boards-replay"` |
| `version` | string | Yes | Format version (e.g., `"1.0"`) |
| `metadata` | ReplayMetadata | Yes | Game context and descriptive information |
| `initialState` | GameStateSnapshot | Yes | Starting game state |
| `events` | EventRecord[] | Yes | Ordered event sequence |
| `finalState` | GameStateSnapshot? | No | Final state (if game ended) |

### ReplayMetadata

Descriptive information about the replay.

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `gameType` | string | Yes | Game identifier (e.g., `"chess"`, `"go"`) |
| `players` | string[] | Yes | Player identifiers |
| `created` | DateTime | Yes | Creation timestamp (UTC) |
| `title` | string? | No | Display title |
| `tags` | string[]? | No | Categorization tags |
| `customMetadata` | Dictionary<string,string>? | No | Game-specific metadata |

### GameStateSnapshot

Immutable state snapshot with integrity hash.

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `hash` | string | Yes | 64-bit FNV-1a hash (hex) |
| `hash128` | string? | No | 128-bit xxHash (hex) |
| `artifacts` | Dictionary<string,object> | Yes | Artifact states by ID |
| `turn` | TurnStateData? | No | Turn information |
| `random` | RandomSourceData? | No | RNG state |

### EventRecord

Recorded event with metadata.

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `index` | int | Yes | 0-based sequence number |
| `type` | string | Yes | Event type name |
| `data` | Dictionary<string,object> | Yes | Event-specific data |
| `timestamp` | DateTime? | No | Event timestamp |
| `resultHash` | string | Yes | State hash after event |
| `resultHash128` | string? | No | 128-bit hash after event |

## Validation

The `Validate` method checks:

1. **Format Version**: Confirms `format` is `"veggerby-boards-replay"`
2. **Required Fields**: Ensures all mandatory properties are present
3. **Event Ordering**: Verifies event indices are sequential (0, 1, 2, ...)
4. **Type Information**: Checks event types are non-empty

Future enhancements will add:
- Hash chain validation (each event's resultHash matches next state)
- Artifact consistency checks
- Game-specific rule validation

## Determinism Guarantees

The replay format preserves determinism through:

1. **State Hashing**: 64-bit and 128-bit hashes verify state integrity
2. **RNG Serialization**: Random source seeds stored for reproducibility
3. **Event Ordering**: Explicit indices prevent reordering
4. **Immutable Design**: All state transitions produce new instances

## Compatibility

### Version 1.0

- **Forward Compatibility**: Newer engines should load v1.0 replays
- **Backward Compatibility**: v1.0 engines can't load future versions
- **Migration**: Future breaking changes will include migration guide

### Cross-Platform

- **Encoding**: UTF-8 JSON ensures consistent text representation
- **Hashing**: Deterministic algorithms (FNV-1a, xxHash) produce same hashes across platforms
- **Timestamps**: ISO 8601 UTC format

## Performance Characteristics

Based on initial implementation (Phase 1):

- **Serialization**: O(n) where n = artifact count + event count
- **Validation**: O(n) event count checks
- **Memory**: Linear with game size; no deep cloning
- **File Size**: ~100-500 bytes per event (JSON overhead)

Future optimizations:
- Compression (gzip): ~60-80% reduction
- Binary format: ~50% reduction + faster parsing
- Streaming: Constant memory for large replays

## Current Limitations (Phase 1)

1. **No Deserialization**: `Deserialize` not yet implemented
   - Requires artifact reconstruction from IDs
   - Needs event factory for polymorphic event creation

2. **Partial Event Support**: Currently handles:
   - ✅ MovePieceGameEvent
   - ⚠️ RollDiceGameEvent (generic, skipped)
   - ❌ Other events (type name only)

3. **No Hash Chain Validation**: Future work will verify:
   - Event result hashes match next state
   - No state divergence during replay

4. **No Module Integration**: Chess/Go/Backgammon-specific states not yet handled

## Future Enhancements

### Phase 2: Complete Deserialization
- Artifact reconstruction from serialized data
- Event factory with type registry
- Full round-trip tests

### Phase 3: Deterministic Replay Engine
- Apply events sequentially with validation
- Hash chain verification
- Divergence detection and reporting

### Phase 4: External Format Support
- PGN import/export (Chess)
- SGF import/export (Go)
- Format conversion utilities

### Phase 5: Advanced Features
- Compression (gzip)
- Binary format (performance)
- Streaming for large replays (1000+ moves)
- Digital signatures (tamper-proofing)

## References

- [Core Concepts](/docs/core-concepts.md)
- [Determinism & RNG Timeline](/docs/determinism-rng-timeline.md)
- [Game Termination](/docs/game-termination.md)
- [GameBuilder Guide](/docs/gamebuilder-guide.md)

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2026-01-07 | Initial format specification and Phase 1 implementation |
