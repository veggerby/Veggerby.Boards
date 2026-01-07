using Veggerby.Boards.States;

namespace Veggerby.Boards.Serialization;

/// <summary>
/// Serializes game progress (state + event history) to and from persistent replay format.
/// </summary>
public interface IGameReplaySerializer
{
    /// <summary>
    /// Serializes the current game progress to a replay envelope.
    /// </summary>
    /// <param name="progress">The game progress to serialize.</param>
    /// <returns>A replay envelope containing serialized state and event history.</returns>
    ReplayEnvelope Serialize(GameProgress progress);

    /// <summary>
    /// Deserializes a replay envelope back to game progress.
    /// </summary>
    /// <param name="envelope">The replay envelope to deserialize.</param>
    /// <returns>The reconstructed game progress.</returns>
    /// <exception cref="ReplayDeserializationException">Thrown when deserialization fails due to invalid or incompatible envelope.</exception>
    GameProgress Deserialize(ReplayEnvelope envelope);

    /// <summary>
    /// Validates a replay envelope for structural integrity, format compatibility, and hash verification.
    /// </summary>
    /// <param name="envelope">The replay envelope to validate.</param>
    /// <returns>A validation result indicating success or specific errors/warnings.</returns>
    ValidationResult Validate(ReplayEnvelope envelope);
}
