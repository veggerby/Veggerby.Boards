﻿using System;

namespace Veggerby.Boards.Core.Builder.Artifacts
{
    public class PlayerDefinition : DefinitionBase
    {
        public PlayerDefinition(GameBuilder builder) : base(builder)
        {
        }

        public string PlayerId { get; private set; }

        public PlayerDefinition WithId(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("Value cannot be null or empty", nameof(id));
            }

            PlayerId = id;
            return this;
        }
    }
}
