using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Flows.Events;

namespace Veggerby.Boards.States;

/// <summary>
/// Manages branching timelines for scenario exploration, enabling "what-if" analysis without losing the main game line.
/// </summary>
/// <remarks>
/// <see cref="BranchingGameHistory"/> extends linear history with full branching support, allowing users to:
/// <list type="bullet">
/// <item><description>Create alternative timelines at any point</description></item>
/// <item><description>Switch between branches</description></item>
/// <item><description>Compare outcomes across different branches</description></item>
/// </list>
/// Branches share common history nodes for memory efficiency through structural sharing.
/// </remarks>
public sealed class BranchingGameHistory
{
    private readonly Dictionary<string, HistoryBranch> _branches;
    private readonly string _currentBranchId;
    private readonly int _nextBranchNumber;

    /// <summary>
    /// Initializes a new instance of the <see cref="BranchingGameHistory"/> class with an initial progress state.
    /// </summary>
    /// <param name="initialProgress">The initial game progress to start the history.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="initialProgress"/> is null.</exception>
    public BranchingGameHistory(GameProgress initialProgress)
    {
        ArgumentNullException.ThrowIfNull(initialProgress);

        var mainBranch = new HistoryBranch("main", "main", initialProgress, new NullGameEvent());
        _branches = new Dictionary<string, HistoryBranch>
        {
            { mainBranch.Id, mainBranch }
        };
        _currentBranchId = mainBranch.Id;
        _nextBranchNumber = 1;
    }

    /// <summary>
    /// Internal constructor for creating new branching history instances.
    /// </summary>
    private BranchingGameHistory(Dictionary<string, HistoryBranch> branches, string currentBranchId, int nextBranchNumber)
    {
        _branches = branches;
        _currentBranchId = currentBranchId;
        _nextBranchNumber = nextBranchNumber;
    }

    /// <summary>
    /// Gets the currently active branch.
    /// </summary>
    public HistoryBranch CurrentBranch => _branches[_currentBranchId];

    /// <summary>
    /// Gets a read-only dictionary of all branches.
    /// </summary>
    public IReadOnlyDictionary<string, HistoryBranch> Branches => _branches;

    /// <summary>
    /// Gets the current game progress at the active position in the current branch.
    /// </summary>
    public GameProgress Current => CurrentBranch.Current;

    /// <summary>
    /// Creates a new branch at the current position in the active branch, producing a new <see cref="BranchingGameHistory"/> instance.
    /// </summary>
    /// <param name="name">The name for the new branch.</param>
    /// <returns>A new branching history instance with the new branch as the active branch.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is null.</exception>
    /// <remarks>
    /// The new branch starts at the current position of the active branch and shares all history nodes
    /// up to that point for memory efficiency. Future changes on either branch create independent timelines.
    /// </remarks>
    public BranchingGameHistory CreateBranch(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        var currentBranch = CurrentBranch;
        var branchId = $"branch-{_nextBranchNumber}";
        var newBranch = currentBranch.Fork(branchId, name);

        var newBranches = new Dictionary<string, HistoryBranch>(_branches)
        {
            { newBranch.Id, newBranch }
        };

        return new BranchingGameHistory(newBranches, newBranch.Id, _nextBranchNumber + 1);
    }

    /// <summary>
    /// Switches to a different branch, producing a new <see cref="BranchingGameHistory"/> instance.
    /// </summary>
    /// <param name="branchId">The ID of the branch to switch to.</param>
    /// <returns>A new branching history instance with the specified branch as active.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the specified branch does not exist.</exception>
    public BranchingGameHistory SwitchToBranch(string branchId)
    {
        if (!_branches.ContainsKey(branchId))
        {
            throw new InvalidOperationException($"Branch {branchId} does not exist");
        }

        return new BranchingGameHistory(_branches, branchId, _nextBranchNumber);
    }

    /// <summary>
    /// Applies a game event to the current branch, producing a new <see cref="BranchingGameHistory"/> instance.
    /// </summary>
    /// <param name="event">The game event to apply.</param>
    /// <returns>A new branching history instance with the event applied to the current branch.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="event"/> is null.</exception>
    public BranchingGameHistory Apply(IGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        var currentBranch = CurrentBranch;
        var updatedBranch = currentBranch.Apply(@event);

        var newBranches = new Dictionary<string, HistoryBranch>(_branches)
        {
            [currentBranch.Id] = updatedBranch
        };

        return new BranchingGameHistory(newBranches, _currentBranchId, _nextBranchNumber);
    }

    /// <summary>
    /// Moves back one step in the current branch, producing a new <see cref="BranchingGameHistory"/> instance.
    /// </summary>
    /// <returns>A new branching history instance with the current branch moved back one step.</returns>
    /// <exception cref="InvalidOperationException">Thrown when already at the start of the branch.</exception>
    public BranchingGameHistory Undo()
    {
        var currentBranch = CurrentBranch;
        var updatedBranch = currentBranch.Undo();

        var newBranches = new Dictionary<string, HistoryBranch>(_branches)
        {
            [currentBranch.Id] = updatedBranch
        };

        return new BranchingGameHistory(newBranches, _currentBranchId, _nextBranchNumber);
    }

    /// <summary>
    /// Moves forward one step in the current branch, producing a new <see cref="BranchingGameHistory"/> instance.
    /// </summary>
    /// <returns>A new branching history instance with the current branch moved forward one step.</returns>
    /// <exception cref="InvalidOperationException">Thrown when already at the end of the branch.</exception>
    public BranchingGameHistory Redo()
    {
        var currentBranch = CurrentBranch;
        var updatedBranch = currentBranch.Redo();

        var newBranches = new Dictionary<string, HistoryBranch>(_branches)
        {
            [currentBranch.Id] = updatedBranch
        };

        return new BranchingGameHistory(newBranches, _currentBranchId, _nextBranchNumber);
    }

    /// <summary>
    /// Jumps to a specific point in the current branch, producing a new <see cref="BranchingGameHistory"/> instance.
    /// </summary>
    /// <param name="index">The zero-based index to navigate to.</param>
    /// <returns>A new branching history instance with the current branch positioned at the specified index.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="index"/> is out of bounds.</exception>
    public BranchingGameHistory GoTo(int index)
    {
        var currentBranch = CurrentBranch;
        var updatedBranch = currentBranch.GoTo(index);

        var newBranches = new Dictionary<string, HistoryBranch>(_branches)
        {
            [currentBranch.Id] = updatedBranch
        };

        return new BranchingGameHistory(newBranches, _currentBranchId, _nextBranchNumber);
    }

    /// <summary>
    /// Gets all events applied to reach the current state in the current branch.
    /// </summary>
    /// <returns>A read-only list of events in chronological order.</returns>
    public IReadOnlyList<IGameEvent> GetEventHistory()
    {
        return CurrentBranch.GetEventHistory();
    }
}

/// <summary>
/// Represents a single branch in a branching timeline.
/// </summary>
public sealed class HistoryBranch
{
    private readonly List<BranchNode> _nodes;
    private readonly int _currentIndex;

    /// <summary>
    /// Initializes a new branch starting at the specified progress state.
    /// </summary>
    /// <param name="id">The unique identifier for this branch.</param>
    /// <param name="name">The human-readable name for this branch.</param>
    /// <param name="initialProgress">The starting progress state.</param>
    /// <param name="initialEvent">The event that created this state (NullGameEvent for initial state).</param>
    internal HistoryBranch(string id, string name, GameProgress initialProgress, IGameEvent initialEvent)
    {
        Id = id;
        Name = name;
        _nodes = new List<BranchNode>
        {
            new BranchNode(initialProgress, initialEvent)
        };
        _currentIndex = 0;
        ParentBranchId = null;
        BranchPointIndex = null;
    }

    /// <summary>
    /// Internal constructor for creating updated branch instances.
    /// </summary>
    private HistoryBranch(string id, string name, List<BranchNode> nodes, int currentIndex, string? parentBranchId, int? branchPointIndex)
    {
        Id = id;
        Name = name;
        _nodes = nodes;
        _currentIndex = currentIndex;
        ParentBranchId = parentBranchId;
        BranchPointIndex = branchPointIndex;
    }

    /// <summary>
    /// Gets the unique identifier for this branch.
    /// </summary>
    public string Id
    {
        get;
    }

    /// <summary>
    /// Gets the human-readable name for this branch.
    /// </summary>
    public string Name
    {
        get;
    }

    /// <summary>
    /// Gets the parent branch ID if this branch was forked from another branch.
    /// </summary>
    public string? ParentBranchId
    {
        get;
    }

    /// <summary>
    /// Gets the index in the parent branch where this branch was created.
    /// </summary>
    public int? BranchPointIndex
    {
        get;
    }

    /// <summary>
    /// Gets the current game progress at the active position in this branch.
    /// </summary>
    public GameProgress Current => _nodes[_currentIndex].Progress;

    /// <summary>
    /// Gets the current index position in this branch (zero-based).
    /// </summary>
    public int CurrentIndex => _currentIndex;

    /// <summary>
    /// Gets the total length of this branch.
    /// </summary>
    public int Length => _nodes.Count;

    /// <summary>
    /// Gets a value indicating whether undo operation is possible (not at start of branch).
    /// </summary>
    public bool CanUndo => _currentIndex > 0;

    /// <summary>
    /// Gets a value indicating whether redo operation is possible (not at end of branch).
    /// </summary>
    public bool CanRedo => _currentIndex < _nodes.Count - 1;

    /// <summary>
    /// Creates a new branch forked from the current position.
    /// </summary>
    /// <param name="branchId">The ID for the new branch.</param>
    /// <param name="branchName">The name for the new branch.</param>
    /// <returns>A new branch instance starting at the current position.</returns>
    internal HistoryBranch Fork(string branchId, string branchName)
    {
        // Create new branch with copy of nodes up to current position
        var newNodes = new List<BranchNode>(_currentIndex + 1);
        for (var i = 0; i <= _currentIndex; i++)
        {
            newNodes.Add(_nodes[i]);
        }

        return new HistoryBranch(branchId, branchName, newNodes, _currentIndex, Id, _currentIndex);
    }

    /// <summary>
    /// Applies a game event to this branch, producing a new branch instance.
    /// </summary>
    internal HistoryBranch Apply(IGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        var newProgress = Current.HandleEvent(@event);

        // Create new node list by copying nodes up to current position, then appending new node
        var newNodes = new List<BranchNode>(_currentIndex + 2);
        for (var i = 0; i <= _currentIndex; i++)
        {
            newNodes.Add(_nodes[i]);
        }

        newNodes.Add(new BranchNode(newProgress, @event));

        return new HistoryBranch(Id, Name, newNodes, _currentIndex + 1, ParentBranchId, BranchPointIndex);
    }

    /// <summary>
    /// Moves back one step in this branch, producing a new branch instance.
    /// </summary>
    internal HistoryBranch Undo()
    {
        if (!CanUndo)
        {
            throw new InvalidOperationException("Cannot undo: at start of branch");
        }

        return new HistoryBranch(Id, Name, _nodes, _currentIndex - 1, ParentBranchId, BranchPointIndex);
    }

    /// <summary>
    /// Moves forward one step in this branch, producing a new branch instance.
    /// </summary>
    internal HistoryBranch Redo()
    {
        if (!CanRedo)
        {
            throw new InvalidOperationException("Cannot redo: at end of branch");
        }

        return new HistoryBranch(Id, Name, _nodes, _currentIndex + 1, ParentBranchId, BranchPointIndex);
    }

    /// <summary>
    /// Jumps to a specific point in this branch, producing a new branch instance.
    /// </summary>
    internal HistoryBranch GoTo(int index)
    {
        if (index < 0 || index >= _nodes.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index), $"Index must be between 0 and {_nodes.Count - 1}");
        }

        return new HistoryBranch(Id, Name, _nodes, index, ParentBranchId, BranchPointIndex);
    }

    /// <summary>
    /// Gets all events applied to reach the current state in this branch.
    /// </summary>
    internal IReadOnlyList<IGameEvent> GetEventHistory()
    {
        var events = new List<IGameEvent>(_currentIndex + 1);
        for (var i = 0; i <= _currentIndex; i++)
        {
            events.Add(_nodes[i].Event);
        }

        return events.AsReadOnly();
    }
}

/// <summary>
/// Represents a single node in a branch timeline.
/// </summary>
/// <param name="Progress">The game progress snapshot at this point.</param>
/// <param name="Event">The event that was applied to reach this state.</param>
internal sealed record BranchNode(GameProgress Progress, IGameEvent Event);
