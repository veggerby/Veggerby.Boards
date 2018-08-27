using Shouldly;
using System;
using Veggerby.Boards.Core;
using Veggerby.Boards.Core.Flows.Phases;
using Veggerby.Boards.Core.States;
using Veggerby.Boards.Core.States.Conditions;
using Veggerby.Boards.Tests.Core.Fakes;
using Xunit;

namespace Veggerby.Boards.Tests.Core
{
    public class GameEngineTests
    {
        public class New
        {
            [Fact]
            public void Should_initialize_gameengine()
            {
                // arrange
                var game = new TestGameBuilder().Compile();
                var state = GameState.New(game, null);
                var gamePhaseRoot = GamePhase.New(1, new NullGameStateCondition());

                // act
                var actual = GameEngine.New(state, gamePhaseRoot);

                // assert
                actual.ShouldNotBeNull();
                actual.Game.ShouldBe(game);
                actual.GamePhaseRoot.ShouldBe(gamePhaseRoot);
                actual.GameState.ShouldBe(state);
                actual.Events.ShouldBeEmpty();
            }

            [Fact]
            public void Should_throw_with_null_state()
            {
                // arrange
                var gamePhaseRoot = GamePhase.New(1, new NullGameStateCondition());

                // act
                var actual = Should.Throw<ArgumentNullException>(() => GameEngine.New(null, gamePhaseRoot));

                // assert
                actual.ParamName.ShouldBe("initialState");
            }

            [Fact]
            public void Should_throw_with_non_initial_state()
            {
                // arrange
                var game = new TestGameBuilder().Compile();
                var state = GameState.New(game, null).Next(null);
                var gamePhaseRoot = GamePhase.New(1, new NullGameStateCondition());

                // act
                var actual = Should.Throw<ArgumentException>(() => GameEngine.New(state, gamePhaseRoot));

                // assert
                actual.ParamName.ShouldBe("initialState");
            }

            [Fact]
            public void Should_throw_with_null_condition()
            {
                // arrange
                var game = new TestGameBuilder().Compile();
                var state = GameState.New(game, null);

                // act
                var actual = Should.Throw<ArgumentNullException>(() => GameEngine.New(state, null));

                // assert
                actual.ParamName.ShouldBe("gamePhaseRoot");
            }
        }
    }
}