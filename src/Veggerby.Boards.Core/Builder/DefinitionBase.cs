using System;

namespace Veggerby.Boards.Core.Builder
{
    public abstract class DefinitionBase
    {
        protected GameBuilder Builder { get; }

        public DefinitionBase(GameBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            Builder = builder;
        }
    }
}
