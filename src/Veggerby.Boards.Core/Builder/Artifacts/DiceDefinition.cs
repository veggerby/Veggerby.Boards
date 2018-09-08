using System;
using Veggerby.Boards.Core.Artifacts;

namespace Veggerby.Boards.Core.Builder.Artifacts
{
    public class DiceDefinition : DefinitionBase
    {
        public DiceDefinition(GameEngineBuilder builder) : base(builder)
        {
        }

        public string DiceId { get; private set; }

        public DiceDefinition WithId(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("Value cannot be null or empty", nameof(id));
            }

            DiceId = id;
            return this;
        }

        public DiceDefinition HasNoValue()
        {
            Builder.AddDiceState(DiceId, null);
            return this;
        }

        public DiceDefinition HasValue(int value)
        {
            Builder.AddDiceState(DiceId, value);
            return this;
        }
    }
}
