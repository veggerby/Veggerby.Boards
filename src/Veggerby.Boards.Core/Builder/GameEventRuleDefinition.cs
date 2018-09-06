namespace Veggerby.Boards.Core.Builder
{
    public class GameEventRuleDefinition : DefinitionBase
    {
        public GameEventRuleDefinition(GameEngineBuilder builder, GamePhaseDefinition gamePhaseDefinitionSettings) : base(builder)
        {
            GamePhaseDefinitionSettings = gamePhaseDefinitionSettings;
        }

        public GamePhaseDefinition GamePhaseDefinitionSettings { get; }
    }
}
