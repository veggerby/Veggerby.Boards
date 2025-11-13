using System.Collections.Generic;
using System.Linq;


using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Builder.Rules;
using Veggerby.Boards.Flows.Phases;

namespace Veggerby.Boards.Builder.Phases;

internal class GamePhaseDefinition(GameBuilder builder, string label) : DefinitionBase(builder), IGamePhaseDefinition, IThenGameEventRule
{
    private int? _number;
    private CompositeGamePhaseConditionDefinition? _conditionDefinition;
    private GameEventRuleDefinitions? _ruleDefinitions;
    private readonly IList<GameEventPreProcessorDefinition> _preProcessorDefinitions = [];
    private readonly string _label = label;
    private string? _exclusivityGroup;

    internal void Add(GameEventPreProcessorDefinition preProcessorDefinition)
    {
        _preProcessorDefinitions.Add(preProcessorDefinition);
    }

    public GamePhaseDefinition WithNumber(int number)
    {
        _number = number;
        return this;
    }

    public GamePhaseDefinition Exclusive(string group)
    {
        _exclusivityGroup = group;
        return this;
    }

    IGamePhaseDefinition IGamePhaseDefinition.Exclusive(string group)
    {
        return Exclusive(group);
    }

    IGamePhaseConditionDefinition IGamePhaseDefinition.If(GameStateConditionFactory factory)
    {
        _conditionDefinition = new CompositeGamePhaseConditionDefinition(Builder, this).Add(new GamePhaseConditionDefinition(Builder, factory, this), null);
        return _conditionDefinition;
    }

    IGamePhaseConditionDefinition IGamePhaseDefinition.If<T>()
    {
        _conditionDefinition = new CompositeGamePhaseConditionDefinition(Builder, this).Add(new GamePhaseConditionDefinition(Builder, game => new T(), this), null);
        return _conditionDefinition;
    }

    IGameEventRuleDefinitionsWithOption IThenGameEventRule.Then()
    {
        _ruleDefinitions = new GameEventRuleDefinitions(Builder, this);
        return _ruleDefinitions;
    }

    internal GamePhase Build(int number, Game game, CompositeGamePhase? parent = null)
    {
        if (_conditionDefinition is null || _ruleDefinitions is null)
        {
            throw new BoardException("Incomplete game phase");
        }

        var condition = _conditionDefinition.Build(game);
        var rule = _ruleDefinitions.Build(game);
        var preprocessors = _preProcessorDefinitions.Select(x => x.Build(game)).ToList();
        return GamePhase.New(_number ?? number, _label, condition, rule, parent, preprocessors, _exclusivityGroup);
    }
}