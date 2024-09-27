using System;
using System.Collections.Generic;
using System.Linq;


using Veggerby.Boards.Core.Artifacts;

namespace Veggerby.Boards.Core.States;

public class GameState
{
    private readonly IDictionary<Artifact, IArtifactState> _childStates;
    private readonly GameState _previousState;

    public IEnumerable<IArtifactState> ChildStates => _childStates.Values.ToList().AsReadOnly();
    public bool IsInitialState => _previousState is null;

    private GameState(IEnumerable<IArtifactState> childStates = null, GameState previousState = null)
    {
        _childStates = (childStates ?? Enumerable.Empty<IArtifactState>()).ToDictionary(x => x.Artifact, x => x);
        _previousState = previousState;
    }

    public T GetState<T>(Artifact artifact) where T : IArtifactState
    {
        return _childStates.ContainsKey(artifact) && _childStates[artifact] is T ? (T)_childStates[artifact] : default(T);
    }

    public IEnumerable<T> GetStates<T>() where T : IArtifactState
    {
        return _childStates.Values.OfType<T>().ToList();
    }

    public bool Equals(GameState other)
    {
        if (other is null)
        {
            return false;
        }

        if (IsInitialState != other.IsInitialState)
        {
            return false;
        }

        if (ChildStates.Count() != other.ChildStates.Count())
        {
            return false;
        }

        return !ChildStates.Except(other.ChildStates).Any();
    }

    public override bool Equals(object obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((GameState)obj);
    }

    public override int GetHashCode()
    {
        var code = new HashCode();
        code.Add(GetType());
        code.Add(IsInitialState);
        foreach (var state in ChildStates)
        {
            code.Add(state);
        }

        return code.ToHashCode();
    }

    public GameState Next(IEnumerable<IArtifactState> newStates)
    {
        return new GameState(
            ChildStates
                .Except(newStates ?? Enumerable.Empty<IArtifactState>(), new ArtifactStateEqualityComparer())
                .Concat(newStates ?? Enumerable.Empty<IArtifactState>()),
            this);
    }

    public IEnumerable<ArtifactStateChange> CompareTo(GameState state)
    {
        if (Equals(state))
        {
            return Enumerable.Empty<ArtifactStateChange>();
        }

        var additions = ChildStates
            .Except(state.ChildStates, new ArtifactStateEqualityComparer())
            .Select(to => new ArtifactStateChange(null, to));

        // states cannot be removed
        /*var removals = state.ChildStates
            .Except(ChildStates, new ArtifactStateEqualityComparer())
            .Select(from => new ArtifactStateChange(from, null));*/

        var changes = state.ChildStates
            .Join(ChildStates, x => x.Artifact, x => x.Artifact, (from, to) => new ArtifactStateChange(from, to))
            .Where(x => !x.From.Equals(x.To));

        return changes.Concat(additions).ToList();
    }

    public static GameState New(IEnumerable<IArtifactState> initialStates)
    {
        return new GameState(initialStates, null);
    }
}