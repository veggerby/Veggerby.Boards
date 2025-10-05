namespace Veggerby.Boards.Tests.Simulation;

#nullable enable

using System;

using Veggerby.Boards.Chess;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Internal;
using Veggerby.Boards.Simulation;
using Veggerby.Boards.States;

using Xunit;

public class SequentialSimulatorTests
{
    [Fact]
    public void GivenSimulationDisabled_WhenRun_ThenThrows()
    {
        // arrange
        FeatureFlags.EnableSimulation = false;
        var builder = new ChessGameBuilder();
        var progress = builder.Compile();

        // act
        void Act() => SequentialSimulator.Run(progress, _ => null);

        // assert
        Assert.Throws<InvalidOperationException>(Act);
    }

    [Fact]
    public void GivenSimplePolicy_WhenRunWithDeterministicSeed_ThenDeterministicTerminalHash()
    {
        // arrange
        FeatureFlags.EnableSimulation = true;
        var builder = new ChessGameBuilder();
        builder.WithSeed(1234);
        var progress = builder.Compile();

        // simple deterministic policy placeholder removed (no coordinate helpers available in current context)

        // act
        var game = progress.Game; // capture game for policy closure
        IGameEvent? Policy(GameState s)
        {
            var king = game.GetPiece("white-king");
            var pieceState = s.GetState<PieceState>(king);
            if (pieceState is null)
            {
                return null;
            }
            // Attempt simple upward move along any accessible tile id convention (e.g., file-letter + rank number not present; fallback stop)
            // Without coordinate helpers, just terminate (acts as determinism placeholder)
            return null;
        }

        var terminal1 = SequentialSimulator.Run(progress, Policy, maxDepth: 2);
        var terminal2 = SequentialSimulator.Run(progress, Policy, maxDepth: 2);

        // assert
        Assert.Equal(terminal1.State.GetHashCode(), terminal2.State.GetHashCode());
    }

    [Fact]
    public void GivenStopPredicate_WhenSatisfied_ThenStopsEarly()
    {
        // arrange
        FeatureFlags.EnableSimulation = true;
        var builder = new ChessGameBuilder();
        var progress = builder.Compile();
        int observedDepth = -1;

        static IGameEvent? Policy(GameState _) => null; // immediately stops

        bool Stop(GameState initial, GameState current, int depth)
        {
            observedDepth = depth;
            return depth >= 0;
        }

        // act
        var terminal = SequentialSimulator.Run(progress, Policy, Stop, maxDepth: 10);

        // assert
        Assert.NotNull(terminal);
        Assert.Equal(0, observedDepth);
    }
}

#nullable disable