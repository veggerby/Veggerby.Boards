using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Serialization;

/// <summary>
/// Validates replay integrity by checking hash chains and event sequence consistency.
/// </summary>
public class ReplayValidator
{
    private readonly JsonReplaySerializer _serializer;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReplayValidator"/> class.
    /// </summary>
    /// <param name="serializer">The serializer for state reconstruction.</param>
    public ReplayValidator(JsonReplaySerializer serializer)
    {
        ArgumentNullException.ThrowIfNull(serializer);

        _serializer = serializer;
    }

    /// <summary>
    /// Validates that a replay can be reconstructed and matches expected hashes.
    /// </summary>
    /// <param name="envelope">The replay envelope to validate.</param>
    /// <param name="initialProgress">The initial GameProgress with engine for replay.</param>
    /// <returns>Validation result with hash chain verification.</returns>
    public ReplayValidationResult ValidateReplay(ReplayEnvelope envelope, GameProgress initialProgress)
    {
        ArgumentNullException.ThrowIfNull(envelope);
        ArgumentNullException.ThrowIfNull(initialProgress);

        var errors = new List<string>();
        var warnings = new List<string>();
        var hashMismatches = new List<HashMismatch>();

        // Validate initial state hash
        if (!string.IsNullOrEmpty(envelope.InitialState.Hash))
        {
            var expectedHash = envelope.InitialState.Hash;
            var actualHash = initialProgress.State.Hash.HasValue
                ? initialProgress.State.Hash.Value.ToString("X16")
                : string.Empty;

            if (actualHash != expectedHash)
            {
                hashMismatches.Add(new HashMismatch
                {
                    EventIndex = -1, // Initial state
                    ExpectedHash = expectedHash,
                    ActualHash = actualHash,
                    Description = "Initial state hash mismatch"
                });
            }
        }

        // Replay events and verify hashes
        var progress = initialProgress;
        for (int i = 0; i < envelope.Events.Count; i++)
        {
            var eventRecord = envelope.Events[i];

            try
            {
                // Deserialize and apply event
                var gameEvent = _serializer.DeserializeEvent(eventRecord);
                progress = progress.HandleEvent(gameEvent);

                // Verify result hash if provided
                if (!string.IsNullOrEmpty(eventRecord.ResultHash))
                {
                    var expectedHash = eventRecord.ResultHash;
                    var actualHash = progress.State.Hash.HasValue
                        ? progress.State.Hash.Value.ToString("X16")
                        : string.Empty;

                    if (actualHash != expectedHash)
                    {
                        hashMismatches.Add(new HashMismatch
                        {
                            EventIndex = i,
                            EventType = eventRecord.Type,
                            ExpectedHash = expectedHash,
                            ActualHash = actualHash,
                            Description = $"Hash mismatch after event {i} ({eventRecord.Type})"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Failed to replay event {i} ({eventRecord.Type}): {ex.Message}");
            }
        }

        // Verify final state hash if provided
        if (envelope.FinalState != null && !string.IsNullOrEmpty(envelope.FinalState.Hash))
        {
            var expectedHash = envelope.FinalState.Hash;
            var actualHash = progress.State.Hash.HasValue
                ? progress.State.Hash.Value.ToString("X16")
                : string.Empty;

            if (actualHash != expectedHash)
            {
                hashMismatches.Add(new HashMismatch
                {
                    EventIndex = envelope.Events.Count,
                    ExpectedHash = expectedHash,
                    ActualHash = actualHash,
                    Description = "Final state hash mismatch"
                });
            }
        }

        // Add hash mismatches to errors
        foreach (var mismatch in hashMismatches)
        {
            errors.Add(mismatch.Description + $" (expected: {mismatch.ExpectedHash}, actual: {mismatch.ActualHash})");
        }

        return new ReplayValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors,
            Warnings = warnings,
            HashMismatches = hashMismatches,
            FinalProgress = progress
        };
    }
}

/// <summary>
/// Result of replay validation including hash chain verification.
/// </summary>
public sealed record ReplayValidationResult
{
    /// <summary>
    /// Gets whether the replay is valid (no errors).
    /// </summary>
    public bool IsValid { get; init; }

    /// <summary>
    /// Gets the list of validation errors.
    /// </summary>
    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Gets the list of validation warnings.
    /// </summary>
    public IReadOnlyList<string> Warnings { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Gets the list of hash mismatches detected during replay.
    /// </summary>
    public IReadOnlyList<HashMismatch> HashMismatches { get; init; } = Array.Empty<HashMismatch>();

    /// <summary>
    /// Gets the final GameProgress after replaying all events (if successful).
    /// </summary>
    public GameProgress? FinalProgress { get; init; }
}

/// <summary>
/// Describes a hash mismatch found during replay validation.
/// </summary>
public sealed record HashMismatch
{
    /// <summary>
    /// Gets the event index where the mismatch occurred (-1 for initial state).
    /// </summary>
    public int EventIndex { get; init; }

    /// <summary>
    /// Gets the event type (if applicable).
    /// </summary>
    public string? EventType { get; init; }

    /// <summary>
    /// Gets the expected hash from the replay envelope.
    /// </summary>
    public string ExpectedHash { get; init; } = string.Empty;

    /// <summary>
    /// Gets the actual hash computed during replay.
    /// </summary>
    public string ActualHash { get; init; } = string.Empty;

    /// <summary>
    /// Gets a human-readable description of the mismatch.
    /// </summary>
    public string Description { get; init; } = string.Empty;
}
