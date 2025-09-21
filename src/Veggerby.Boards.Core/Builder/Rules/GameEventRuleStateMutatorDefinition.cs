using System;
using System.Collections.Generic;
using System.Linq;


using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.Flows.Mutators;

namespace Veggerby.Boards.Core.Builder.Rules;

/// <summary>
/// Fluent builder component for configuring before and after state mutators on a game event rule.
/// </summary>
/// <typeparam name="T">Event type the rule handles.</typeparam>
public class GameEventRuleStateMutatorDefinition<T> : DefinitionBase, IGameEventRuleStateMutatorDefinition<T> where T : IGameEvent
{
    /// <summary>
    /// Initializes a new instance linking to the parent rule definitions collection.
    /// </summary>
    public GameEventRuleStateMutatorDefinition(GameBuilder builder, IGameEventRuleDefinitions parent) : base(builder)
    {
        ArgumentNullException.ThrowIfNull(parent);

        Parent = parent;
    }

    private readonly IList<StateMutatorFactory<T>> _onBeforeMutators = [];
    private readonly IList<StateMutatorFactory<T>> _onAfterMutators = [];

    /// <summary>
    /// Gets the parent rule definitions to allow chaining additional event specific rules.
    /// </summary>
    public IGameEventRuleDefinitions Parent { get; }

    IGameEventRuleStateMutatorDefinition<T> IGameEventRuleStateMutatorDefinitionBefore<T>.Before(StateMutatorFactory<T> mutator)
    {
        _onBeforeMutators.Add(mutator);
        return this;
    }

    IGameEventRuleStateMutatorDefinition<T> IGameEventRuleStateMutatorDefinitionBefore<T>.Before<TMutator>()
    {
        _onBeforeMutators.Add(game => new TMutator());
        return this;
    }

    IGameEventRuleStateMutatorDefinitionDo<T> IGameEventRuleStateMutatorDefinitionDo<T>.Do(StateMutatorFactory<T> mutator)
    {
        _onAfterMutators.Add(mutator);
        return this;
    }

    IGameEventRuleStateMutatorDefinitionDo<T> IGameEventRuleStateMutatorDefinitionDo<T>.Do<TMutator>()
    {
        _onAfterMutators.Add(game => new TMutator());
        return this;
    }

    IGameEventRuleDefinition<TEvent> IGameEventRuleDefinitions.ForEvent<TEvent>()
    {
        return Parent.ForEvent<TEvent>();
    }

    /// <summary>
    /// Builds a composite mutator or single mutator from the registered factories.
    /// </summary>
    private static IStateMutator<T> BuildMutator(IEnumerable<StateMutatorFactory<T>> mutatorFactories, Game game)
    {
        if (!(mutatorFactories?.Any() ?? false))
        {
            return null;
        }

        if (mutatorFactories.Count() == 1)
        {
            return mutatorFactories.Single()(game);
        }

        var mutators = mutatorFactories.Select(x => x(game)).ToArray();
        return new CompositeStateMutator<T>(mutators);
    }

    /// <summary>
    /// Builds the before mutator chain.
    /// </summary>
    internal IStateMutator<T> BuildBefore(Game game)
    {
        return BuildMutator(_onBeforeMutators, game); ;
    }

    /// <summary>
    /// Builds the after mutator chain.
    /// </summary>
    internal IStateMutator<T> BuildAfter(Game game)
    {
        return BuildMutator(_onAfterMutators, game); ;
    }
}