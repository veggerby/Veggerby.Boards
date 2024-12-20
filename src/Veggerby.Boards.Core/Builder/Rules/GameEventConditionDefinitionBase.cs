﻿using System;


using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.Flows.Rules.Conditions;

namespace Veggerby.Boards.Core.Builder.Rules;

internal abstract class GameEventConditionDefinitionBase<T> : DefinitionBase where T : IGameEvent
{
    public GameEventConditionDefinitionBase(GameBuilder builder, IThenStateMutator<T> parent) : base(builder)
    {
        ArgumentNullException.ThrowIfNull(parent);

        Parent = parent;
    }

    public IThenStateMutator<T> Parent { get; }

    internal abstract IGameEventCondition<T> Build(Game game);
}