using System;
using System.Collections.Generic;
using System.Linq;

namespace Veggerby.Boards.Core.Builder.Artifacts
{
    public class PieceDirectionPatternDefinition : DefinitionBase
    {
        private readonly PieceDefinition _pieceDefinition;

        public PieceDirectionPatternDefinition(GameBuilder builder, PieceDefinition pieceDefinition) : base(builder)
        {
            _pieceDefinition = pieceDefinition;
            IsRepeatable = false;
        }

        public string PieceId => _pieceDefinition.PieceId;
        public bool IsRepeatable { get; private set; }
        public IEnumerable<string> DirectionIds { get; private set; }

        public PieceDefinition CanRepeat()
        {
            IsRepeatable = true;
            return _pieceDefinition;
        }

        public PieceDefinition DoesNotRepeat()
        {
            IsRepeatable = false;
            return _pieceDefinition;
        }

        public PieceDirectionPatternDefinition WithDirection(params string[] directions)
        {
            if (directions == null)
            {
                throw new ArgumentNullException(nameof(directions));
            }

            if (!directions.Any())
            {
                throw new ArgumentException("Must provide at least one direction", nameof(directions));
            }

            if (directions.Any(x => string.IsNullOrEmpty(x)))
            {
                throw new ArgumentException("All directions must be non-null and non-empty", nameof(directions));
            }

            DirectionIds = (DirectionIds ?? Enumerable.Empty<string>()).Concat(directions).ToList();
            return this;
        }

        public PieceDefinition Done()
        {
            return _pieceDefinition;
        }
    }
}
