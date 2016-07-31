using System;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Artifacts
{
    public class Die<T> : Artifact
    {
        private readonly IDieValueGenerator<T> _valueGenerator;

        public T Roll(DieState<T> currentState)
        {
            if (currentState != null && !currentState.Artifact.Equals(this))
            {
                throw new ArgumentException("Invalid current state artifact", nameof(currentState));
            }

            return _valueGenerator.GetValue(currentState);
        }

        public Die(string id, IDieValueGenerator<T> valueGenerator) : base(id)
        {
            if (valueGenerator == null)
            {
                throw new ArgumentNullException(nameof(valueGenerator));
            }

            _valueGenerator = valueGenerator;
        }
    }
}