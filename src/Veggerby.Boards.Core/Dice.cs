using System.Collections.Generic;
using System.Linq;

namespace Veggerby.Boards.Core
{
    public class Dice<T> : Die<IEnumerable<T>>
    {
        public new T MinValue => _dice.Min(x => x.MinValue);
        public new T MaxValue => _dice.Max(x => x.MaxValue);

        private readonly IEnumerable<Die<T>> _dice;

        public override IEnumerable<T> Roll() 
        {
            return _dice.Select(x => x.Roll()).ToList();
        }

        public Dice(string id, IEnumerable<Die<T>> dice) : base(id) 
        {
            _dice = (dice ?? Enumerable.Empty<Die<T>>()).ToList();
        }
    }
}