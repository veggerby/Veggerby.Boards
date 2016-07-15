using Veggerby.Boards.Core.Artifacts;

namespace Veggerby.Boards.Backgammon
{
    public class DoublingCube : Die<int>
    {
        public DoublingCube(string id) : base(id, new DoublingCubeValueGenerator())
        {
        }
    }
}