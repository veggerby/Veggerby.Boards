using System;
using System.Collections.Generic;

namespace Veggerby.Boards.Serialization;

/// <summary>
/// Result of replay envelope validation indicating validity and any errors or warnings.
/// </summary>
public sealed record ValidationResult
{
    /// <summary>
    /// Gets a value indicating whether the replay envelope is valid.
    /// </summary>
    public bool IsValid { get; init; }

    /// <summary>
    /// Gets the list of validation errors that prevent successful replay.
    /// </summary>
    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Gets the list of validation warnings that may affect replay quality or compatibility.
    /// </summary>
    public IReadOnlyList<string> Warnings { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Creates a successful validation result.
    /// </summary>
    public static ValidationResult Success() => new() { IsValid = true };

    /// <summary>
    /// Creates a failed validation result with the specified errors.
    /// </summary>
    public static ValidationResult Failed(params string[] errors) =>
        new() { IsValid = false, Errors = errors };

    /// <summary>
    /// Creates a validation result with warnings but still valid.
    /// </summary>
    public static ValidationResult WithWarnings(params string[] warnings) =>
        new() { IsValid = true, Warnings = warnings };
}
