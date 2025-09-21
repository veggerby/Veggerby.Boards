using System.Collections.Generic;
using System.Linq;

namespace Veggerby.Boards;

/// <summary>
/// Possible outcomes of a condition evaluation.
/// </summary>
public enum ConditionResult
{
    /// <summary>
    /// Condition chose not to participate (neutral).
    /// </summary>
    Ignore,
    /// <summary>
    /// Condition validated successfully.
    /// </summary>
    Valid,
    /// <summary>
    /// Condition failed validation.
    /// </summary>
    Invalid
}

/// <summary>
/// Immutable value representing a condition evaluation result with optional reason.
/// </summary>
public class ConditionResponse
{
    /// <summary>
    /// Shared instance representing an ignored/not-applicable result.
    /// </summary>
    public static readonly ConditionResponse NotApplicable = ConditionResponse.New(ConditionResult.Ignore);
    /// <summary>
    /// Shared instance representing a valid result (no reason).
    /// </summary>
    public static readonly ConditionResponse Valid = ConditionResponse.New(ConditionResult.Valid);
    /// <summary>
    /// Shared instance representing an invalid result (no reason).
    /// </summary>
    public static readonly ConditionResponse Invalid = ConditionResponse.New(ConditionResult.Invalid);

    /// <summary>
    /// Gets the result classification.
    /// </summary>
    public ConditionResult Result { get; }
    /// <summary>
    /// Gets the optional textual reason.
    /// </summary>
    public string Reason { get; }

    private ConditionResponse(ConditionResult result, string reason)
    {
        Result = result;
        Reason = reason;
    }

    /// <summary>
    /// Indicates equality based on <see cref="Result"/> only (reason is ignored).
    /// </summary>
    public bool Equals(ConditionResponse other)
    {
        return other is not null && Result == other.Result;
    }

    /// <inheritdoc />
    public override bool Equals(object obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((ConditionResponse)obj);
    }

    /// <inheritdoc />
    public override int GetHashCode() => Result.GetHashCode();

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{GetType().Name} {Result}/{Reason}";
    }

    /// <summary>
    /// Creates a new response instance (not cached).
    /// </summary>
    /// <param name="result">Result value.</param>
    /// <param name="reason">Optional reason.</param>
    public static ConditionResponse New(ConditionResult result, string reason = null)
    {
        return new ConditionResponse(result, reason);
    }

    /// <summary>
    /// Creates a valid result with reason.
    /// </summary>
    public static ConditionResponse Success(string reason)
    {
        return New(ConditionResult.Valid, reason);
    }

    /// <summary>
    /// Creates an invalid result with reason.
    /// </summary>
    public static ConditionResponse Fail(string reason)
    {
        return New(ConditionResult.Invalid, reason);
    }

    /// <summary>
    /// Aggregates multiple failure reasons into a single fail response.
    /// </summary>
    public static ConditionResponse Fail(IEnumerable<ConditionResponse> failures)
    {
        return New(ConditionResult.Invalid, string.Join(",", failures.Select(x => x.Reason)));
    }

    /// <summary>
    /// Creates an ignore result with reason.
    /// </summary>
    public static ConditionResponse Ignore(string reason)
    {
        return New(ConditionResult.Ignore, reason);
    }
}