using System.Collections.Generic;
using System.Linq;

namespace Veggerby.Boards.Core.Flows.Rules
{
    public enum RuleCheckResult
    {
        Ignore,
        Valid,
        Invalid
    }

    public class RuleCheckState
    {
        public static RuleCheckState NotApplicable = RuleCheckState.New(RuleCheckResult.Ignore);
        public static RuleCheckState Valid = RuleCheckState.New(RuleCheckResult.Valid);
        public static RuleCheckState Invalid = RuleCheckState.New(RuleCheckResult.Invalid);

        public RuleCheckResult Result { get; }
        public string Reason { get; }

        private RuleCheckState(RuleCheckResult result, string reason)
        {
            Result = result;
            Reason = reason;
        }

        protected bool Equals(RuleCheckState other)
        {
            return string.Equals(Result, other.Result);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((RuleCheckState)obj);
        }

        public override int GetHashCode()
        {
            return Result.GetHashCode();
        }

        public override string ToString()
        {
            return $"{GetType().Name} {Result}/{Reason}";
        }

        public static RuleCheckState New(RuleCheckResult result, string reason = null)
        {
            return new RuleCheckState(result, reason);
        }

        public static RuleCheckState Success(string reason)
        {
            return New(RuleCheckResult.Valid, reason);
        }

        public static RuleCheckState Fail(string reason)
        {
            return New(RuleCheckResult.Invalid, reason);
        }

        public static RuleCheckState Fail(IEnumerable<RuleCheckState> failures)
        {
            return New(RuleCheckResult.Invalid, string.Join(",", failures.Select(x => x.Reason)));
        }

        public static RuleCheckState Ignore(string reason)
        {
            return New(RuleCheckResult.Ignore, reason);
        }
    }
}