using System.Collections.Generic;
using System.Linq;

namespace Veggerby.Boards.Core;

public enum ConditionResult
{
    Ignore,
    Valid,
    Invalid
}

public class ConditionResponse
{
    public static readonly ConditionResponse NotApplicable = ConditionResponse.New(ConditionResult.Ignore);
    public static readonly ConditionResponse Valid = ConditionResponse.New(ConditionResult.Valid);
    public static readonly ConditionResponse Invalid = ConditionResponse.New(ConditionResult.Invalid);

    public ConditionResult Result { get; }
    public string Reason { get; }

    private ConditionResponse(ConditionResult result, string reason)
    {
        Result = result;
        Reason = reason;
    }

    public bool Equals(ConditionResponse other)
    {
        return other is not null && Result == other.Result;
    }

    public override bool Equals(object obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((ConditionResponse)obj);
    }

    public override int GetHashCode() => Result.GetHashCode();

    public override string ToString()
    {
        return $"{GetType().Name} {Result}/{Reason}";
    }

    public static ConditionResponse New(ConditionResult result, string reason = null)
    {
        return new ConditionResponse(result, reason);
    }

    public static ConditionResponse Success(string reason)
    {
        return New(ConditionResult.Valid, reason);
    }

    public static ConditionResponse Fail(string reason)
    {
        return New(ConditionResult.Invalid, reason);
    }

    public static ConditionResponse Fail(IEnumerable<ConditionResponse> failures)
    {
        return New(ConditionResult.Invalid, string.Join(",", failures.Select(x => x.Reason)));
    }

    public static ConditionResponse Ignore(string reason)
    {
        return New(ConditionResult.Ignore, reason);
    }
}