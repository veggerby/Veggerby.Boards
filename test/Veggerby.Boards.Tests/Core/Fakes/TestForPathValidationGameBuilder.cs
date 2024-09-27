using Veggerby.Boards.Core;

namespace Veggerby.Boards.Tests.Core.Fakes
{
    public class TestForPathValidationGameBuilder : GameBuilder
    {
        private readonly int? _diceValue1;
        private readonly int? _diceValue2;

        public TestForPathValidationGameBuilder(int? diceValue1, int? diceValue2)
        {
            _diceValue1 = diceValue1;
            _diceValue2 = diceValue2;
        }

        protected override void Build()
        {
            // Game + state

            BoardId = "test";

            if (_diceValue1 == null)
            {
                AddDice("dice-1").HasNoValue();
            }
            else
            {
                AddDice("dice-1").HasValue(_diceValue1.Value);
            }

            if (_diceValue2 == null)
            {
                AddDice("dice-2").HasNoValue();
            }
            else
            {
                AddDice("dice-2").HasValue(_diceValue2.Value);
            }

            AddPlayer("player-1");

            AddDirection("clockwise");
            AddDirection("counterclockwise");

            AddTile("tile-1").WithRelationTo("tile-2").InDirection("clockwise");
            AddTile("tile-2")
                .WithRelationTo("tile-1").InDirection("counterclockwise").Done()
                .WithRelationTo("tile-3").InDirection("clockwise");
            AddTile("tile-3")
                .WithRelationTo("tile-2").InDirection("counterclockwise").Done()
                .WithRelationTo("tile-4").InDirection("clockwise");
            AddTile("tile-4")
                .WithRelationTo("tile-3").InDirection("counterclockwise").Done()
                .WithRelationTo("tile-5").InDirection("clockwise");
            AddTile("tile-5")
                .WithRelationTo("tile-4").InDirection("counterclockwise").Done()
                .WithRelationTo("tile-6").InDirection("clockwise");
            AddTile("tile-6")
                .WithRelationTo("tile-5").InDirection("counterclockwise");

            AddPiece("piece-1").WithOwner("player-1").HasDirection("clockwise").CanRepeat().OnTile("tile-1");
        }
    }
}