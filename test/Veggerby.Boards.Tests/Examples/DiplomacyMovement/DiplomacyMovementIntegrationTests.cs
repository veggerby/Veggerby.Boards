using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Events;
using Veggerby.Boards.Examples.DiplomacyMovement;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Tests.Examples.DiplomacyMovement;

/// <summary>
/// Integration tests for simplified Diplomacy movement example demonstrating simultaneous orders with conflict resolution.
/// </summary>
public class DiplomacyMovementIntegrationTests
{
    [Fact]
    public void GivenDiplomacyGame_WhenTwoPlayersMoveToDifferentTerritories_ThenBothMovesSucceed()
    {
        // arrange
        var builder = new DiplomacyMovementGameBuilder();
        var progress = builder.Compile();

        var england = progress.Game.Players.First(p => p.Id == "england");
        var france = progress.Game.Players.First(p => p.Id == "france");

        var englandArmy = progress.Game.GetPiece("england-army-1");
        var franceArmy = progress.Game.GetPiece("france-army-1");

        var edinburgh = progress.Game.GetTile("edinburgh");
        var marseilles = progress.Game.GetTile("marseilles");

        // Initialize staged events state
        var stagedArtifact = new StagedEventsArtifact("staged-events");
        var pendingPlayers = new HashSet<Player> { england, france };
        var stagedState = new StagedEventsState(
            stagedArtifact,
            new Dictionary<Player, Flows.Events.IGameEvent>(),
            pendingPlayers);

        var stateWithStaged = progress.State.Next([stagedState]);
        progress = new GameProgress(progress.Engine, stateWithStaged, null);

        // act
        // England commits: Move army to Edinburgh
        progress = progress.HandleEvent(
            new CommitActionEvent(england, new MoveOrderEvent(england, englandArmy!, edinburgh!)));

        // France commits: Move army to Marseilles
        progress = progress.HandleEvent(
            new CommitActionEvent(france, new MoveOrderEvent(france, franceArmy!, marseilles!)));

        // Verify both commitments are recorded
        var staged = progress.State.GetStates<StagedEventsState>().Single();
        staged.IsComplete.Should().BeTrue();
        staged.Commitments.Should().HaveCount(2);

        // Reveal commitments
        progress = progress.HandleEvent(new RevealCommitmentsEvent());

        // assert
        // Staged state should be cleared
        progress.State.GetStates<StagedEventsState>().Should().BeEmpty();

        // Both units should have moved to their destinations
        var positions = progress.State.GetStates<PieceState>().ToList();

        var englandPosition = positions.First(ps => ps.Artifact.Equals(englandArmy));
        var francePosition = positions.First(ps => ps.Artifact.Equals(franceArmy));

        englandPosition.CurrentTile.Should().Be(edinburgh);
        francePosition.CurrentTile.Should().Be(marseilles);
    }

    [Fact]
    public void GivenDiplomacyGame_WhenCommitmentsInReverseOrder_ThenSameOutcome()
    {
        // arrange
        // This test verifies determinism: commitment order doesn't affect outcome
        var builder = new DiplomacyMovementGameBuilder();
        var progress1 = builder.Compile();
        var progress2 = builder.Compile();

        var england1 = progress1.Game.Players.First(p => p.Id == "england");
        var france1 = progress1.Game.Players.First(p => p.Id == "france");
        var england2 = progress2.Game.Players.First(p => p.Id == "england");
        var france2 = progress2.Game.Players.First(p => p.Id == "france");

        var englandArmy1 = progress1.Game.GetPiece("england-army-1");
        var franceArmy1 = progress1.Game.GetPiece("france-army-1");
        var englandArmy2 = progress2.Game.GetPiece("england-army-1");
        var franceArmy2 = progress2.Game.GetPiece("france-army-1");

        var edinburgh1 = progress1.Game.GetTile("edinburgh");
        var marseilles1 = progress1.Game.GetTile("marseilles");
        var edinburgh2 = progress2.Game.GetTile("edinburgh");
        var marseilles2 = progress2.Game.GetTile("marseilles");

        // Setup first game
        var stagedArtifact1 = new StagedEventsArtifact("staged-events");
        var stagedState1 = new StagedEventsState(
            stagedArtifact1,
            new Dictionary<Player, Flows.Events.IGameEvent>(),
            new HashSet<Player> { england1, france1 });
        progress1 = new GameProgress(progress1.Engine, progress1.State.Next([stagedState1]), null);

        // Setup second game
        var stagedArtifact2 = new StagedEventsArtifact("staged-events");
        var stagedState2 = new StagedEventsState(
            stagedArtifact2,
            new Dictionary<Player, Flows.Events.IGameEvent>(),
            new HashSet<Player> { england2, france2 });
        progress2 = new GameProgress(progress2.Engine, progress2.State.Next([stagedState2]), null);

        // act
        // Version 1: England commits first
        progress1 = progress1.HandleEvent(
            new CommitActionEvent(england1, new MoveOrderEvent(england1, englandArmy1!, edinburgh1!)));
        progress1 = progress1.HandleEvent(
            new CommitActionEvent(france1, new MoveOrderEvent(france1, franceArmy1!, marseilles1!)));
        progress1 = progress1.HandleEvent(new RevealCommitmentsEvent());

        // Version 2: France commits first
        progress2 = progress2.HandleEvent(
            new CommitActionEvent(france2, new MoveOrderEvent(france2, franceArmy2!, marseilles2!)));
        progress2 = progress2.HandleEvent(
            new CommitActionEvent(england2, new MoveOrderEvent(england2, englandArmy2!, edinburgh2!)));
        progress2 = progress2.HandleEvent(new RevealCommitmentsEvent());

        // assert
        var positions1 = progress1.State.GetStates<PieceState>().ToList();
        var positions2 = progress2.State.GetStates<PieceState>().ToList();

        var englandPos1 = positions1.First(ps => ps.Artifact.Equals(englandArmy1));
        var francePos1 = positions1.First(ps => ps.Artifact.Equals(franceArmy1));
        var englandPos2 = positions2.First(ps => ps.Artifact.Equals(englandArmy2));
        var francePos2 = positions2.First(ps => ps.Artifact.Equals(franceArmy2));

        // Outcomes should be identical regardless of commitment order
        englandPos1.CurrentTile.Id.Should().Be(englandPos2.CurrentTile.Id);
        francePos1.CurrentTile.Id.Should().Be(francePos2.CurrentTile.Id);
    }
}
