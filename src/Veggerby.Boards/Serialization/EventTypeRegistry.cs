using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Flows.Events;

namespace Veggerby.Boards.Serialization;

/// <summary>
/// Registry for mapping event type names to factory functions for deserialization.
/// </summary>
public class EventTypeRegistry
{
    private readonly Dictionary<string, Func<IReadOnlyDictionary<string, object>, IGameEvent>> _factories = new();

    /// <summary>
    /// Registers an event type with a factory function.
    /// </summary>
    /// <param name="typeName">The event type name (e.g., "MovePieceGameEvent").</param>
    /// <param name="factory">Factory function that creates an event from serialized data.</param>
    public void Register(string typeName, Func<IReadOnlyDictionary<string, object>, IGameEvent> factory)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(typeName);
        ArgumentNullException.ThrowIfNull(factory);

        _factories[typeName] = factory;
    }

    /// <summary>
    /// Creates an event instance from serialized data.
    /// </summary>
    /// <param name="typeName">The event type name.</param>
    /// <param name="data">The serialized event data.</param>
    /// <returns>The deserialized event instance.</returns>
    /// <exception cref="ReplayDeserializationException">Thrown when the type is not registered.</exception>
    public IGameEvent Create(string typeName, IReadOnlyDictionary<string, object> data)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(typeName);
        ArgumentNullException.ThrowIfNull(data);

        if (!_factories.TryGetValue(typeName, out var factory))
        {
            throw new ReplayDeserializationException($"Event type '{typeName}' is not registered");
        }

        try
        {
            return factory(data);
        }
        catch (Exception ex)
        {
            throw new ReplayDeserializationException($"Failed to deserialize event type '{typeName}'", ex);
        }
    }

    /// <summary>
    /// Checks if an event type is registered.
    /// </summary>
    /// <param name="typeName">The event type name.</param>
    /// <returns>True if registered; false otherwise.</returns>
    public bool IsRegistered(string typeName)
    {
        return _factories.ContainsKey(typeName);
    }

    /// <summary>
    /// Creates a default registry with core event types.
    /// </summary>
    /// <param name="game">The game instance for artifact resolution.</param>
    /// <returns>A registry with MovePieceGameEvent registered.</returns>
    public static EventTypeRegistry CreateDefault(Artifacts.Game game)
    {
        ArgumentNullException.ThrowIfNull(game);

        var registry = new EventTypeRegistry();

        // Register MovePieceGameEvent
        registry.Register("MovePieceGameEvent", data =>
        {
            var pieceId = data["PieceId"]?.ToString() ?? throw new ReplayDeserializationException("Missing PieceId");

            // Handle JsonElement for PathTileIds
            object? pathTileIdsObj = data.ContainsKey("PathTileIds") ? data["PathTileIds"] : null;
            if (pathTileIdsObj is not System.Text.Json.JsonElement pathTileIds)
            {
                throw new ReplayDeserializationException("Missing or invalid PathTileIds");
            }

            var piece = game.GetArtifactById(pieceId) as Artifacts.Piece ?? throw new ReplayDeserializationException($"Piece '{pieceId}' not found");

            var tileIds = new List<string>();
            foreach (var element in pathTileIds.EnumerateArray())
            {
                tileIds.Add(element.GetString() ?? throw new ReplayDeserializationException("Invalid tile ID in path"));
            }

            // Build TilePath from tile IDs
            var relations = new List<Artifacts.Relations.TileRelation>();
            for (int i = 0; i < tileIds.Count - 1; i++)
            {
                // Try artifacts first, then board tiles
                var fromTile = game.GetArtifactById(tileIds[i]) as Artifacts.Tile;
                if (fromTile == null)
                {
                    fromTile = game.Board.Tiles.FirstOrDefault(t => t.Id == tileIds[i])
                        ?? throw new ReplayDeserializationException($"Tile '{tileIds[i]}' not found");
                }

                var toTile = game.GetArtifactById(tileIds[i + 1]) as Artifacts.Tile;
                if (toTile == null)
                {
                    toTile = game.Board.Tiles.FirstOrDefault(t => t.Id == tileIds[i + 1])
                        ?? throw new ReplayDeserializationException($"Tile '{tileIds[i + 1]}' not found");
                }

                var relation = game.Board.TileRelations.Where(r => r.From.Equals(fromTile) && r.To.Equals(toTile)).FirstOrDefault()
                    ?? throw new ReplayDeserializationException($"No relation found from '{tileIds[i]}' to '{tileIds[i + 1]}'");

                relations.Add(relation);
            }

            var path = new Artifacts.Relations.TilePath(relations);
            return new MovePieceGameEvent(piece, path);
        });

        return registry;
    }
}
