using System.Collections.Generic;

namespace Veggerby.Boards.Core
{
    public abstract class Rule<T> where T : Artifact
    {
        public abstract IEnumerable<State<T>> GetValidstates(T artifact);
    }
}