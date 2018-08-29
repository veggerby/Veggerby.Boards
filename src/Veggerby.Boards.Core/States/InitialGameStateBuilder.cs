using System.Collections.Generic;
using System.Linq;
using Veggerby.Boards.Core.Artifacts;

namespace Veggerby.Boards.Core.States
{
    public abstract class InitialGameStateBuilder
    {
        public abstract void Build(Game game);

        private IDictionary<string, int?> _diceState = new Dictionary<string, int?>();

        private IDictionary<string, string> _piecePositions = new Dictionary<string, string>();

        protected void AddNullDice(string diceId)
        {
            _diceState.Add(diceId, null);
        }

        protected void AddDiceValue(string diceId, int value)
        {
            _diceState.Add(diceId, value);
        }

        protected void AddPieceOnTile(string pieceId, string tileId)
        {
            _piecePositions.Add(pieceId, tileId);
        }

        public GameState Compile(Game game)
        {
            Build(game);

            var pieceStates = _piecePositions
                .Select(x => new PieceState(game.GetPiece(x.Key), game.GetTile(x.Value)))
                .ToList();

            var diceStates = _diceState
                .Select(x => x.Value == null
                    ? (IArtifactState)new NullDiceState<int>(game.GetArtifact<RegularDice>(x.Key))
                    : (IArtifactState)new DiceState<int>(game.GetArtifact<RegularDice>(x.Key), x.Value.Value))
                .ToList();

            return GameState.New(game, pieceStates.Concat(diceStates).ToList());
        }
    }
}