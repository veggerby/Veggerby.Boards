using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Builder.Fluent;
using Veggerby.Boards.Builder.Rules;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;
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
    private RulePriority _priority = RulePriority.Normal;
    private string? _strategyIdentifier;
    private ConflictResolutionStrategy _conflictResolution = ConflictResolutionStrategy.FirstWins;
    private GameStateConditionFactory? _endGameConditionFactory;
    private Func<Game, IStateMutator<IGameEvent>>? _endGameMutatorFactory;

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

    IGamePhaseDefinition IGamePhaseDefinition.WithPriority(RulePriority priority)
    {
        return WithPriority(priority);
    }

    IGamePhaseDefinition IGamePhaseDefinition.WithStrategy(string identifier)
    {
        return WithStrategy(identifier);
    }

    IGamePhaseDefinition IGamePhaseDefinition.WithConflictResolution(ConflictResolutionStrategy strategy)
    {
        return WithConflictResolution(strategy);
    }

    /// <summary>
    /// Sets the priority level for this phase's rule.
    /// </summary>
    /// <param name="priority">Priority level.</param>
    /// <returns>This phase definition for fluent chaining.</returns>
    public GamePhaseDefinition WithPriority(RulePriority priority)
    {
        _priority = priority;
        return this;
    }

    /// <summary>
    /// Sets an optional strategy identifier for grouping related rules.
    /// </summary>
    /// <param name="identifier">Strategy identifier (e.g., "castling", "en-passant").</param>
    /// <returns>This phase definition for fluent chaining.</returns>
    public GamePhaseDefinition WithStrategy(string identifier)
    {
        _strategyIdentifier = identifier;
        return this;
    }

    /// <summary>
    /// Sets the conflict resolution strategy for this phase.
    /// </summary>
    /// <param name="strategy">Conflict resolution strategy.</param>
    /// <returns>This phase definition for fluent chaining.</returns>
    public GamePhaseDefinition WithConflictResolution(ConflictResolutionStrategy strategy)
    {
        _conflictResolution = strategy;
        return this;
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

    /// <summary>
    /// Configures automatic endgame detection for this phase.
    /// </summary>
    /// <param name="conditionFactory">Factory producing the endgame condition.</param>
    /// <param name="mutatorFactory">Factory producing the mutator that adds terminal states.</param>
    /// <returns>This phase definition for fluent chaining.</returns>
    public IGamePhaseDefinition WithEndGameDetection(GameStateConditionFactory conditionFactory, Func<Game, IStateMutator<IGameEvent>> mutatorFactory)
    {
        _endGameConditionFactory = conditionFactory;
        _endGameMutatorFactory = mutatorFactory;
        return this;
    }

    IGameEventRuleDefinitionsWithOption IThenGameEventRule.Then()
    {
        _ruleDefinitions = new GameEventRuleDefinitions(Builder, this);
        return _ruleDefinitions;
    }

    IGamePhaseDefinition IThenGameEventRule.DefineRules(Action<IPhaseRuleBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure, nameof(configure));

        // Initialize rule definitions if not already done
        if (_ruleDefinitions is null)
        {
            _ruleDefinitions = new GameEventRuleDefinitions(Builder, this);
        }

        // Create the scoped phase rule builder and let the lambda configure it
        var phaseBuilder = new PhaseRuleBuilder(Builder, this, _ruleDefinitions);
        configure(phaseBuilder);

        return this;
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

        var endGameCondition = _endGameConditionFactory?.Invoke(game);
        var endGameMutator = _endGameMutatorFactory?.Invoke(game);

        return GamePhase.New(_number ?? number, _label, condition, rule, parent, preprocessors, _exclusivityGroup, _priority, _strategyIdentifier, _conflictResolution, endGameCondition, endGameMutator);
    }
}