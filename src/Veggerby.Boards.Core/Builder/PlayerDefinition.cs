﻿using System;

namespace Veggerby.Boards.Core.Builder
{
    public class PlayerDefinition : DefinitionBase
    {
        public PlayerDefinition(GameEngineBuilder builder) : base(builder)
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