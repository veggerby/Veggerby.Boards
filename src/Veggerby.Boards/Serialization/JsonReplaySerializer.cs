using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Random;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Serialization;

/// <summary>
/// JSON-based implementation of <see cref="IGameReplaySerializer"/> for serializing and deserializing game replays.
/// </summary>
/// <remarks>
/// This serializer produces human-readable JSON suitable for debugging, sharing, and long-term storage.
/// State hashes are encoded as hex strings; artifact states are serialized as property dictionaries.
/// </remarks>
public class JsonReplaySerializer : IGameReplaySerializer
{
    private readonly Game _game;
    private readonly string _gameType;
    private readonly EventTypeRegistry _eventRegistry;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonReplaySerializer"/> class.
    /// </summary>
    /// <param name="game">The game definition (required for artifact and event type resolution during deserialization).</param>
    /// <param name="gameType">The game type identifier (e.g., "chess", "go"). If null, uses "default".</param>
    /// <param name="eventRegistry">Optional event type registry. If null, uses default registry.</param>
    public JsonReplaySerializer(Game game, string? gameType = null, EventTypeRegistry? eventRegistry = null)
    {
        ArgumentNullException.ThrowIfNull(game);

        _game = game;
        _gameType = gameType ?? "default";
        _eventRegistry = eventRegistry ?? EventTypeRegistry.CreateDefault(game);
    }

    /// <inheritdoc />
    public ReplayEnvelope Serialize(GameProgress progress)
    {
        ArgumentNullException.ThrowIfNull(progress);

        var initialSnapshot = SerializeState(progress.State);
        var eventRecords = new List<EventRecord>();

        // Serialize event chain
        var index = 0;
        foreach (var evt in progress.Events)
        {
            var eventData = SerializeEvent(evt);
            eventRecords.Add(new EventRecord
            {
                Index = index++,
                Type = evt.GetType().Name,
                Data = eventData,
                Timestamp = DateTime.UtcNow, // TODO: Capture original event timestamp when event model is enhanced
                ResultHash = string.Empty, // Will be computed during replay validation
                ResultHash128 = null
            });
        }

        // Extract players from active player states
        var players = progress.State.GetStates<ActivePlayerState>()
            .Select(s => s.Artifact.Id)
            .ToList();

        var metadata = new ReplayMetadata
        {
            GameType = _gameType,
            Players = players,
            Created = DateTime.UtcNow
        };

        GameStateSnapshot? finalSnapshot = null;
        // Check if game has ended (look for GameEndedState marker)
        var gameEndedStates = progress.State.GetStates<GameEndedState>().ToList();
        if (gameEndedStates.Count > 0)
        {
            finalSnapshot = SerializeState(progress.State);
        }

        return new ReplayEnvelope
        {
            Metadata = metadata,
            InitialState = initialSnapshot,
            Events = eventRecords,
            FinalState = finalSnapshot
        };
    }

    /// <inheritdoc />
    public GameProgress Deserialize(ReplayEnvelope envelope)
    {
        ArgumentNullException.ThrowIfNull(envelope);

        // Validate envelope first
        var validation = Validate(envelope);
        if (!validation.IsValid)
        {
            throw new ReplayDeserializationException($"Invalid envelope: {string.Join(", ", validation.Errors)}");
        }

        // Note: We cannot fully reconstruct GameProgress without the original GameBuilder/GameEngine
        // This is a limitation of Phase 2 - we can deserialize the state and events,
        // but cannot replay them without the full rule engine.
        // For now, we'll deserialize just the state and event list.

        // Deserialize initial state
        var initialState = DeserializeState(envelope.InitialState);

        // Deserialize events into a list (but we can't replay them without the engine)
        var events = new List<IGameEvent>();
        foreach (var eventRecord in envelope.Events)
        {
            events.Add(DeserializeEvent(eventRecord));
        }

        // TODO: To fully implement replay, we need either:
        // 1. The original GameBuilder to be serialized/reconstructed, or
        // 2. A way to create a GameEngine from just the Game artifact

        throw new NotImplementedException(
            "Full GameProgress reconstruction not yet supported. " +
            "Use ReconstructState() for state-only deserialization, " +
            "or ReplayValidator.ValidateReplay() with an existing GameProgress instance that provides the required engine context.");
    }

    /// <summary>
    /// Reconstructs just the GameState from a replay envelope without full GameProgress/Engine context.
    /// </summary>
    /// <param name="envelope">The replay envelope.</param>
    /// <returns>The deserialized GameState.</returns>
    public GameState ReconstructState(ReplayEnvelope envelope)
    {
        ArgumentNullException.ThrowIfNull(envelope);

        // Use finalState if available, otherwise initialState
        var snapshot = envelope.FinalState ?? envelope.InitialState;
        return DeserializeState(snapshot);
    }

    /// <inheritdoc />
    public ValidationResult Validate(ReplayEnvelope envelope)
    {
        ArgumentNullException.ThrowIfNull(envelope);

        var errors = new List<string>();
        var warnings = new List<string>();

        // Validate format
        if (envelope.Format != "veggerby-boards-replay")
        {
            errors.Add($"Invalid format: expected 'veggerby-boards-replay', got '{envelope.Format}'");
        }

        // Validate version
        if (string.IsNullOrWhiteSpace(envelope.Version))
        {
            errors.Add("Missing format version");
        }
        else if (envelope.Version != "1.0")
        {
            warnings.Add($"Unknown format version '{envelope.Version}' - may not be compatible");
        }

        // Validate metadata
        if (string.IsNullOrWhiteSpace(envelope.Metadata?.GameType))
        {
            errors.Add("Missing game type in metadata");
        }

        // Validate initial state
        if (envelope.InitialState is null)
        {
            errors.Add("Missing initial state");
        }
        else if (string.IsNullOrWhiteSpace(envelope.InitialState.Hash))
        {
            warnings.Add("Initial state missing hash - cannot verify integrity");
        }

        // Validate events
        if (envelope.Events is null)
        {
            errors.Add("Missing events collection");
        }
        else
        {
            for (int i = 0; i < envelope.Events.Count; i++)
            {
                var evt = envelope.Events[i];
                if (evt.Index != i)
                {
                    errors.Add($"Event index mismatch at position {i}: expected {i}, got {evt.Index}");
                }

                if (string.IsNullOrWhiteSpace(evt.Type))
                {
                    errors.Add($"Event at index {i} missing type");
                }
            }
        }

        return errors.Count > 0
            ? ValidationResult.Failed(errors.ToArray())
            : warnings.Count > 0
                ? ValidationResult.WithWarnings(warnings.ToArray())
                : ValidationResult.Success();
    }

    private GameStateSnapshot SerializeState(GameState state)
    {
        var artifacts = new Dictionary<string, object>();

        foreach (var artifactState in state.ChildStates)
        {
            var stateData = SerializeArtifactState(artifactState);
            artifacts[artifactState.Artifact.Id] = stateData;
        }

        // Look for turn state artifacts
        var turnStates = state.GetStates<TurnState>().ToList();
        TurnStateData? turnStateData = null;
        if (turnStates.Count > 0)
        {
            var turnData = turnStates[0];
            turnStateData = new TurnStateData
            {
                Player = string.Empty, // TODO: TurnState doesn't track active player directly - derive from ActivePlayerState
                Number = turnData.TurnNumber,
                Segment = turnData.Segment.ToString()
            };
        }

        RandomSourceData? randomData = null;
        if (state.Random is not null)
        {
            randomData = new RandomSourceData
            {
                Seed = state.Random.Seed,
                TypeName = state.Random.GetType().Name
            };
        }

        return new GameStateSnapshot
        {
            Hash = state.Hash.HasValue ? state.Hash.Value.ToString("X16") : string.Empty,
            Hash128 = state.Hash128.HasValue ? $"{state.Hash128.Value.Low:X16}{state.Hash128.Value.High:X16}" : null,
            Artifacts = artifacts,
            Turn = turnStateData,
            Random = randomData
        };
    }

    private Dictionary<string, object> SerializeArtifactState(IArtifactState artifactState)
    {
        var data = new Dictionary<string, object>
        {
            ["Type"] = artifactState.GetType().Name,
            ["ArtifactId"] = artifactState.Artifact.Id,
            ["ArtifactType"] = artifactState.Artifact.GetType().Name
        };

        // Serialize specific state types
        switch (artifactState)
        {
            case PieceState pieceState:
                data["CurrentTile"] = pieceState.CurrentTile.Id;
                break;

            case ActivePlayerState activePlayerState:
                // No additional properties beyond artifact
                break;

            case CapturedPieceState:
                // CapturedPieceState only has the artifact, no additional properties
                break;

            case NullDiceState:
                // No additional properties
                break;

            case DiceState<int> diceStateInt:
                data["Value"] = diceStateInt.CurrentValue;
                break;

            case TurnState turnState:
                data["Number"] = turnState.TurnNumber;
                data["Segment"] = turnState.Segment.ToString();
                data["PassStreak"] = turnState.PassStreak;
                break;

            case ExtrasState extrasState:
                // Serialize the wrapped value using JSON serialization
                var extrasJson = JsonSerializer.Serialize(extrasState.Value);
                data["ExtrasJson"] = extrasJson;
                data["ExtrasTypeName"] = extrasState.Value?.GetType().FullName ?? string.Empty;
                break;
        }

        return data;
    }

    private Dictionary<string, object> SerializeEvent(IGameEvent evt)
    {
        var data = new Dictionary<string, object>();

        switch (evt)
        {
            case MovePieceGameEvent moveEvent:
                data["PieceId"] = moveEvent.Piece.Id;
                data["FromTileId"] = moveEvent.From.Id;
                data["ToTileId"] = moveEvent.To.Id;
                data["PathTileIds"] = moveEvent.Path.Tiles.Select(t => t.Id).ToList();
                break;

            // RollDiceGameEvent is generic, skip for now
            // case RollDiceGameEvent<T> rollEvent:
            //     data["DiceIds"] = rollEvent.NewDiceStates.Select(d => d.Artifact.Id).ToList();
            //     break;

            default:
                // For unknown event types, serialize type name only
                // Future: use reflection to serialize properties
                break;
        }

        return data;
    }

    private GameState DeserializeState(GameStateSnapshot snapshot)
    {
        var states = new List<IArtifactState>();

        // Deserialize each artifact state
        foreach (var kvp in snapshot.Artifacts)
        {
            var artifactId = kvp.Key;
            var stateDataObj = kvp.Value;

            // Handle JsonElement from deserialized JSON
            JsonElement stateData;
            if (stateDataObj is JsonElement je)
            {
                stateData = je;
            }
            else
            {
                // If it's already a dictionary, we need to convert it
                throw new ReplayDeserializationException($"Unexpected state data type for artifact '{artifactId}'");
            }

            var stateType = stateData.GetProperty("Type").GetString() ?? throw new ReplayDeserializationException("Missing state Type");
            var artifact = _game.GetArtifactById(artifactId) ?? throw new ReplayDeserializationException($"Artifact '{artifactId}' not found");

            var state = DeserializeArtifactState(stateType, stateData, artifact);
            if (state != null)
            {
                states.Add(state);
            }
        }

        // Reconstruct random source if present
        IRandomSource? random = null;
        if (snapshot.Random != null)
        {
            random = XorShiftRandomSource.Create(snapshot.Random.Seed);
        }

        return GameState.New(states, random);
    }

    private IArtifactState? DeserializeArtifactState(string stateType, JsonElement stateData, Artifact artifact)
    {
        // Handle generic types (e.g., "DiceState`1" -> "DiceState")
        // TODO: Use reflection-based type resolution or dictionary mapping for better maintainability
        var baseTypeName = stateType.Contains('`') ? stateType.Substring(0, stateType.IndexOf('`')) : stateType;

        switch (baseTypeName)
        {
            case "PieceState":
                var currentTileId = stateData.GetProperty("CurrentTile").GetString() ?? throw new ReplayDeserializationException("Missing CurrentTile");

                // Try to find tile in game artifacts first, then in board tiles
                var tile = _game.GetArtifactById(currentTileId) as Tile;
                if (tile == null)
                {
                    tile = _game.Board.Tiles.FirstOrDefault(t => t.Id == currentTileId)
                        ?? throw new ReplayDeserializationException($"Tile '{currentTileId}' not found");
                }

                return new PieceState((Piece)artifact, tile);

            case "ActivePlayerState":
                // ActivePlayerState has IsActive property - not in current serialization, assume active
                return new ActivePlayerState((Player)artifact, true);

            case "CapturedPieceState":
                return new CapturedPieceState((Piece)artifact);

            case "NullDiceState":
                return new NullDiceState((Dice)artifact);

            case "DiceState":
                // DiceState<T> - we need to deserialize the value
                // For now, we'll create a DiceState<int> assuming that's the most common
                if (stateData.TryGetProperty("Value", out var valueElement))
                {
                    var value = valueElement.GetInt32();
                    return new DiceState<int>((Dice)artifact, value);
                }

                // If no value property, return NullDiceState
                return new NullDiceState((Dice)artifact);

            case "TurnState":
                // Deserialize turn state
                var turnNumber = stateData.GetProperty("Number").GetInt32();
                var segmentStr = stateData.GetProperty("Segment").GetString() ?? "Main";
                var segment = Enum.Parse<TurnSegment>(segmentStr);
                return new TurnState((TurnArtifact)artifact, turnNumber, segment);

            case "ExtrasState":
                // ExtrasState requires deserialization of the wrapped value
                // For now, skip or handle specific known types
                if (stateData.TryGetProperty("ExtrasJson", out var extrasJsonElement))
                {
                    var extrasJson = extrasJsonElement.GetString();
                    if (!string.IsNullOrEmpty(extrasJson))
                    {
                        // TODO: Deserialize based on ExtrasTypeName
                        // For now, skip ExtrasState deserialization
                    }
                }

                return null;

            default:
                throw new ReplayDeserializationException($"Unknown state type '{stateType}'");
        }
    }

    internal IGameEvent DeserializeEvent(EventRecord eventRecord)
    {
        return _eventRegistry.Create(eventRecord.Type, eventRecord.Data);
    }
}
