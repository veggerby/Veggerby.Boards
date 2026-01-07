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

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonReplaySerializer"/> class.
    /// </summary>
    /// <param name="game">The game definition (required for artifact and event type resolution during deserialization).</param>
    /// <param name="gameType">The game type identifier (e.g., "chess", "go"). If null, uses "default".</param>
    public JsonReplaySerializer(Game game, string? gameType = null)
    {
        ArgumentNullException.ThrowIfNull(game);

        _game = game;
        _gameType = gameType ?? "default";
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
                Timestamp = DateTime.UtcNow, // Note: original timestamp not captured in current event model
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

        throw new NotImplementedException("Deserialization not yet implemented - requires artifact reconstruction and event factory");
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
                Player = string.Empty, // TurnState doesn't track player directly
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
}
