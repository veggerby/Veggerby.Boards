﻿using System;

namespace Veggerby.Boards.Core.Builder.Artifacts
{
    public class DirectionDefinition : DefinitionBase
    {
        public DirectionDefinition(GameBuilder builder) : base(builder)
        {
        }

        public string DirectionId { get; private set; }

        public DirectionDefinition WithId(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("Value cannot be null or empty", nameof(id));
            }

            DirectionId = id;
            return this;
        }
    }
}
