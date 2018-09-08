using System.Linq;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Builder;
using Veggerby.Boards.Core.Builder.Phases;
using Veggerby.Boards.Core.Builder.Rules;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.Flows.Rules;
using Veggerby.Boards.Core.Flows.Rules.Conditions;
using Veggerby.Boards.Core.States.Conditions;

namespace Veggerby.Boards.Core
{
    public static class GameEngineBuilderHelperExtensions
    {
        public static GameEventRuleDefinition<RollDiceGameEvent<T>> IfEventHasDice<T>(this GameEventRuleDefinition<RollDiceGameEvent<T>> definition, params string[] diceIds)
        {
            return definition.If(game => new DiceGameEventCondition<T>(game.GetArtifacts<Dice<T>>(diceIds)));
        }

        public static GameEventRuleDefinition<RollDiceGameEvent<T>> AndEventHasDice<T>(this GameEventRuleDefinition<RollDiceGameEvent<T>> definition, params string[] diceIds)
        {
            return definition.And(game => new DiceGameEventCondition<T>(game.GetArtifacts<Dice<T>>(diceIds)));
        }

        public static GameEventRuleDefinition<RollDiceGameEvent<T>> OrEventHasDice<T>(this GameEventRuleDefinition<RollDiceGameEvent<T>> definition, params string[] diceIds)
        {
            return definition.Or(game => new DiceGameEventCondition<T>(game.GetArtifacts<Dice<T>>(diceIds)));
        }

        public static GameEventRuleDefinition<MovePieceGameEvent> IfTileIsNot(this GameEventRuleDefinition<MovePieceGameEvent> definition, params string[] tileIds)
        {
            return definition.If(game => new TileExceptionGameEventCondition(tileIds.Select(tileId => game.GetTile(tileId)).ToArray()));
        }

        public static GameEventRuleDefinition<MovePieceGameEvent> AndTileIsNot(this GameEventRuleDefinition<MovePieceGameEvent> definition, params string[] tileIds)
        {
            return definition.And(game => new TileExceptionGameEventCondition(tileIds.Select(tileId => game.GetTile(tileId)).ToArray()));
        }

        public static GameEventRuleDefinition<MovePieceGameEvent> OrTileIsNot(this GameEventRuleDefinition<MovePieceGameEvent> definition, params string[] tileIds)
        {
            return definition.Or(game => new TileExceptionGameEventCondition(tileIds.Select(tileId => game.GetTile(tileId)).ToArray()));
        }

        public static GameEventRuleDefinition<T> IfPlayerHasNoPiecesOn<T>(this GameEventRuleDefinition<T> definition, params string[] tileIds) where T : IGameEvent
        {
            return definition.If(game => new NoPiecesOnTilesGameEventCondition<T>(tileIds.Select(tileId => game.GetTile(tileId)).ToArray()));
        }

        public static GameEventRuleDefinition<T> AndPlayerHasNoPiecesOn<T>(this GameEventRuleDefinition<T> definition, params string[] tileIds) where T : IGameEvent
        {
            return definition.And(game => new NoPiecesOnTilesGameEventCondition<T>(tileIds.Select(tileId => game.GetTile(tileId)).ToArray()));
        }


        public static GameEventRuleDefinition<T> OrPlayerHasNoPiecesOn<T>(this GameEventRuleDefinition<T> definition, params string[] tileIds) where T : IGameEvent
        {
            return definition.Or(game => new NoPiecesOnTilesGameEventCondition<T>(tileIds.Select(tileId => game.GetTile(tileId)).ToArray()));
        }

        public static GameEventRuleDefinition<MovePieceGameEvent> IfTileIsNotBlocked(this GameEventRuleDefinition<MovePieceGameEvent> definition, int numberOfPiecesToBlock = 2, PlayerOption occupiedBy = PlayerOption.Opponent)
        {
            return definition.If(game => new TileBlockedGameEventCondition(numberOfPiecesToBlock, occupiedBy));
        }

        public static GameEventRuleDefinition<MovePieceGameEvent> AndTileIsNotBlocked(this GameEventRuleDefinition<MovePieceGameEvent> definition, int numberOfPiecesToBlock = 2, PlayerOption occupiedBy = PlayerOption.Opponent)
        {
            return definition.And(game => new TileBlockedGameEventCondition(numberOfPiecesToBlock, occupiedBy));
        }

        public static GameEventRuleDefinition<MovePieceGameEvent> OrTileIsNotBlocked(this GameEventRuleDefinition<MovePieceGameEvent> definition, int numberOfPiecesToBlock = 2, PlayerOption occupiedBy = PlayerOption.Opponent)
        {
            return definition.Or(game => new TileBlockedGameEventCondition(numberOfPiecesToBlock, occupiedBy));
        }

        public static GamePhaseConditionDefinition IfAllDiceAreRolled<T>(this GamePhaseDefinition definition, params string[] diceIds)
        {
            return definition.If(game => new DiceGameStateCondition<Dice<T>, T>(game.GetArtifacts<Dice<T>>(diceIds), CompositeMode.All));
        }

        public static GamePhaseConditionDefinition AndAllDiceAreRolled<T>(this GamePhaseConditionDefinition definition, params string[] diceIds)
        {
            return definition.And(game => new DiceGameStateCondition<Dice<T>, T>(game.GetArtifacts<Dice<T>>(diceIds), CompositeMode.All));
        }

        public static GamePhaseConditionDefinition OrAllDiceAreRolled<T>(this GamePhaseConditionDefinition definition, params string[] diceIds)
        {
            return definition.Or(game => new DiceGameStateCondition<Dice<T>, T>(game.GetArtifacts<Dice<T>>(diceIds), CompositeMode.All));
        }

        public static GamePhaseConditionDefinition IfAnyDiceIsRolled<T>(this GamePhaseDefinition definition, params string[] diceIds)
        {
            return definition.If(game => new DiceGameStateCondition<Dice<T>, T>(game.GetArtifacts<Dice<T>>(diceIds), CompositeMode.Any));
        }

        public static GamePhaseConditionDefinition AndAnyDiceIsRolled<T>(this GamePhaseConditionDefinition definition, params string[] diceIds)
        {
            return definition.And(game => new DiceGameStateCondition<Dice<T>, T>(game.GetArtifacts<Dice<T>>(diceIds), CompositeMode.Any));
        }

        public static GamePhaseConditionDefinition OrAnyDiceIsRolled<T>(this GamePhaseConditionDefinition definition, params string[] diceIds)
        {
            return definition.Or(game => new DiceGameStateCondition<Dice<T>, T>(game.GetArtifacts<Dice<T>>(diceIds), CompositeMode.Any));
        }

        public static GamePhaseConditionDefinition IfHasOneActivePlayer(this GamePhaseDefinition definition)
        {
            return definition.If<SingleActivePlayerGameStateCondition>();
        }

        public static GamePhaseConditionDefinition AndHasOneActivePlayer(this GamePhaseConditionDefinition definition)
        {
            return definition.And<SingleActivePlayerGameStateCondition>();
        }

        public static GamePhaseConditionDefinition OrHasOneActivePlayer(this GamePhaseConditionDefinition definition)
        {
            return definition.Or<SingleActivePlayerGameStateCondition>();
        }

        public static GamePhaseConditionDefinition AndIsInitialGameState(this GamePhaseConditionDefinition definition)
        {
            return definition.And<InitialGameStateCondition>();
        }

        public static GamePhaseConditionDefinition IfIsInitialGameState(this GamePhaseDefinition definition)
        {
            return definition.If<InitialGameStateCondition>();
        }

        public static GamePhaseConditionDefinition OrIsInitialGameState(this GamePhaseConditionDefinition definition)
        {
            return definition.Or<InitialGameStateCondition>();
        }
    }
}