using System;

namespace Veggerby.Boards.Core.Builder
{
    public abstract class DefinitionBase
    {
        protected GameEngineBuilder Builder { get; }

        public DefinitionBase(GameEngineBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            Builder = builder;
        }
    }
}
