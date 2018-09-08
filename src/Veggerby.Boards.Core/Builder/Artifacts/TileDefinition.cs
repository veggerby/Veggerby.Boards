using System;

namespace Veggerby.Boards.Core.Builder.Artifacts
{
    public class TileDefinition : DefinitionBase
    {
        public TileDefinition(GameEngineBuilder builder) : base(builder)
        {
        }

        public string TileId { get; private set; }

        public TileDefinition WithId(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("Value cannot be null or empty", nameof(id));
            }

            TileId = id;
            return this;
        }

        public TileRelationDefinition WithRelationTo(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("Value cannot be null or empty", nameof(id));
            }

            var relation = new TileRelationDefinition(Builder, this)
                .FromTile(this.TileId)
                .ToTile(id);

            Builder.Add(relation);
            return relation;
        }

        public TileRelationDefinition WithRelationFrom(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("Value cannot be null or empty", nameof(id));
            }

            var relation = new TileRelationDefinition(Builder, this)
                .FromTile(id)
                .ToTile(this.TileId);

            Builder.Add(relation);
            return relation;
        }
    }
}
