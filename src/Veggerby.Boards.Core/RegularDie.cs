using System;

namespace Veggerby.Boards.Core
{
    public class RegularDie : Die<int>
    {
        public override int Roll()
        {
            return new Random().Next(MinValue, MaxValue);
        }

        public RegularDie(string id) : base(id, 1, 6)
        {
        }
    }
}