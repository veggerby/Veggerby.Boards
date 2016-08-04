namespace Veggerby.Boards.Core.Artifacts
{
    public class RegularDie : Die<int>
    {
        public RegularDie(string id, int sides = 6) : base(id, new RandomDieValueGenerator(1, sides))
        {
        }
    }
}