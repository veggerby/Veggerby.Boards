using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Serialization;
using Veggerby.Boards.Tests.Core.Fakes;

namespace Veggerby.Boards.Tests.Core.Serialization;

/// <summary>
/// Tests for <see cref="JsonReplaySerializer"/> covering serialization, validation, and basic round-trip scenarios.
/// </summary>
public class JsonReplaySerializerTests
{
    [Fact]
    public void Serialize_WithEmptyProgress_ShouldProduceValidEnvelope()
    {
        // arrange
        var builder = new TestGameBuilder();
        var progress = builder.Compile();
        var serializer = new JsonReplaySerializer(progress.Game, "test-game");

        // act
        var envelope = serializer.Serialize(progress);

        // assert
        envelope.Should().NotBeNull();
        envelope.Format.Should().Be("veggerby-boards-replay");
        envelope.Version.Should().Be("1.0");
        envelope.Metadata.GameType.Should().Be("test-game");
        envelope.InitialState.Should().NotBeNull();
        envelope.Events.Should().NotBeNull();
        envelope.Events.Should().BeEmpty();
    }

    [Fact]
    public void Serialize_WithProgress_ShouldIncludeInitialStateHash()
    {
        // arrange
        var builder = new TestGameBuilder();
        var progress = builder.Compile();
        var serializer = new JsonReplaySerializer(progress.Game, "test-game");

        // act
        var envelope = serializer.Serialize(progress);

        // assert
        envelope.InitialState.Hash.Should().NotBeNullOrWhiteSpace();
        envelope.InitialState.Hash128.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Validate_WithValidEnvelope_ShouldReturnSuccess()
    {
        // arrange
        var builder = new TestGameBuilder();
        var progress = builder.Compile();
        var serializer = new JsonReplaySerializer(progress.Game, "test-game");
        var envelope = serializer.Serialize(progress);

        // act
        var result = serializer.Validate(envelope);

        // assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WithInvalidFormat_ShouldReturnError()
    {
        // arrange
        var builder = new TestGameBuilder();
        var progress = builder.Compile();
        var serializer = new JsonReplaySerializer(progress.Game, "test-game");
        var envelope = new ReplayEnvelope
        {
            Format = "invalid-format",
            Metadata = new ReplayMetadata { GameType = "test-game" },
            InitialState = new GameStateSnapshot()
        };

        // act
        var result = serializer.Validate(envelope);

        // assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Invalid format"));
    }

    [Fact]
    public void Validate_WithMissingGameType_ShouldReturnError()
    {
        // arrange
        var builder = new TestGameBuilder();
        var progress = builder.Compile();
        var serializer = new JsonReplaySerializer(progress.Game, "test-game");
        var envelope = new ReplayEnvelope
        {
            Metadata = new ReplayMetadata { GameType = string.Empty },
            InitialState = new GameStateSnapshot()
        };

        // act
        var result = serializer.Validate(envelope);

        // assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Missing game type"));
    }

    [Fact]
    public void Validate_WithUnknownVersion_ShouldReturnWarning()
    {
        // arrange
        var builder = new TestGameBuilder();
        var progress = builder.Compile();
        var serializer = new JsonReplaySerializer(progress.Game, "test-game");
        var envelope = new ReplayEnvelope
        {
            Version = "2.0",
            Metadata = new ReplayMetadata { GameType = "test-game" },
            InitialState = new GameStateSnapshot()
        };

        // act
        var result = serializer.Validate(envelope);

        // assert
        result.IsValid.Should().BeTrue();
        result.Warnings.Should().Contain(w => w.Contains("Unknown format version"));
    }

    [Fact]
    public void Validate_WithMismatchedEventIndices_ShouldReturnError()
    {
        // arrange
        var builder = new TestGameBuilder();
        var progress = builder.Compile();
        var serializer = new JsonReplaySerializer(progress.Game, "test-game");
        var envelope = new ReplayEnvelope
        {
            Metadata = new ReplayMetadata { GameType = "test-game" },
            InitialState = new GameStateSnapshot(),
            Events = new List<EventRecord>
            {
                new EventRecord { Index = 0, Type = "Event1" },
                new EventRecord { Index = 2, Type = "Event2" } // Should be index 1
            }
        };

        // act
        var result = serializer.Validate(envelope);

        // assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Event index mismatch"));
    }

    [Fact]
    public void Serialize_WithMovePieceEvent_ShouldIncludeEventInEnvelope()
    {
        // arrange
        var builder = new TestGameBuilder();
        var progress = builder.Compile();

        // Get artifacts from the built game
        var piece = progress.Game.Artifacts.OfType<Piece>().First(p => p.Id == "piece-1");
        var tile1 = progress.Game.Board.Tiles.First(t => t.Id == "tile-1");
        var tile2 = progress.Game.Board.Tiles.First(t => t.Id == "tile-2");

        // Create a move event
        var relation = progress.Game.Board.TileRelations.Single(r => r.From.Equals(tile1) && r.To.Equals(tile2));
        var path = new TilePath([relation]);
        var moveEvent = new MovePieceGameEvent(piece, path);

        // Handle the event to add it to progress
        progress = progress.HandleEvent(moveEvent);

        var serializer = new JsonReplaySerializer(progress.Game, "test-game");

        // act
        var envelope = serializer.Serialize(progress);

        // assert
        envelope.Events.Should().HaveCount(1);
        envelope.Events[0].Type.Should().Be("MovePieceGameEvent");
        envelope.Events[0].Index.Should().Be(0);
        envelope.Events[0].Data.Should().ContainKey("PieceId");
        envelope.Events[0].Data["PieceId"].Should().Be("piece-1");
    }

    [Fact]
    public void Serialize_WithMultipleEvents_ShouldPreserveOrder()
    {
        // arrange
        var builder = new TestGameBuilder();
        var progress = builder.Compile();

        // Get artifacts from the built game
        var piece = progress.Game.Artifacts.OfType<Piece>().First(p => p.Id == "piece-1");
        var tile1 = progress.Game.Board.Tiles.First(t => t.Id == "tile-1");
        var tile2 = progress.Game.Board.Tiles.First(t => t.Id == "tile-2");

        // Create two move events
        var relation1 = progress.Game.Board.TileRelations.Single(r => r.From.Equals(tile1) && r.To.Equals(tile2));
        var path1 = new TilePath([relation1]);
        var moveEvent1 = new MovePieceGameEvent(piece, path1);
        progress = progress.HandleEvent(moveEvent1);

        var relation2 = progress.Game.Board.TileRelations.Single(r => r.From.Equals(tile2) && r.To.Equals(tile1));
        var path2 = new TilePath([relation2]);
        var moveEvent2 = new MovePieceGameEvent(piece, path2);
        progress = progress.HandleEvent(moveEvent2);

        var serializer = new JsonReplaySerializer(progress.Game, "test-game");

        // act
        var envelope = serializer.Serialize(progress);

        // assert
        envelope.Events.Should().HaveCount(2);
        envelope.Events[0].Index.Should().Be(0);
        envelope.Events[1].Index.Should().Be(1);
        envelope.Events[0].Data["ToTileId"].Should().Be("tile-2");
        envelope.Events[1].Data["ToTileId"].Should().Be("tile-1");
    }

    [Fact]
    public void ValidationResult_Success_ShouldHaveIsValidTrue()
    {
        // arrange

        // act
        var result = ValidationResult.Success();

        // assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
        result.Warnings.Should().BeEmpty();
    }

    [Fact]
    public void ValidationResult_Failed_ShouldHaveIsValidFalse()
    {
        // arrange

        // act
        var result = ValidationResult.Failed("Error 1", "Error 2");

        // assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
        result.Errors.Should().Contain("Error 1");
        result.Errors.Should().Contain("Error 2");
    }

    [Fact]
    public void ValidationResult_WithWarnings_ShouldHaveIsValidTrue()
    {
        // arrange

        // act
        var result = ValidationResult.WithWarnings("Warning 1", "Warning 2");

        // assert
        result.IsValid.Should().BeTrue();
        result.Warnings.Should().HaveCount(2);
        result.Warnings.Should().Contain("Warning 1");
        result.Warnings.Should().Contain("Warning 2");
    }
}
