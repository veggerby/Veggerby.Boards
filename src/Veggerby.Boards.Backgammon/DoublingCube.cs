namespace Veggerby.Boards.Core.Artifacts
{
    public class DoublingCube : Die<int>
    {
        public DoublingCube(string id) : base(id, new DoublingCubeValueGenerator())
        {
        }
    }
}