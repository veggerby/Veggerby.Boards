using System.Collections.Generic;

namespace Veggerby.Boards.Core.Artifacts
{
    public class Deck : CompositeArtifact<Card>
    {
        public Deck(string id, IEnumerable<Card> cards) : base(id, cards)
        {
        }
    }
}