using Shouldly;
using System;
using Veggerby.Boards.Core;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.Flows.Phases;
using Veggerby.Boards.Core.Flows.Rules;
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
                var builder = new TestGameBuilder().Compile();
                var game = builder.Game;
                var gamePhaseRoot = GamePhase.New(1, "test", new NullGameStateCondition(), GameEventRule<IGameEvent>.Null);

                // act
                var actual = new GameEngine(game, gamePhaseRoot);

                // assert
                actual.ShouldNotBeNull();
                actual.Game.ShouldBe(game);
                actual.GamePhaseRoot.ShouldBe(gamePhaseRoot);
            }

            [Fact]
            public void Should_throw_with_null_state()
            {
                // arrange
                var gamePhaseRoot = GamePhase.New(1, "test", new NullGameStateCondition(), GameEventRule<IGameEvent>.Null);

                // act
                var actual = Should.Throw<ArgumentNullException>(() => new GameEngine(null, gamePhaseRoot));

                // assert
                actual.ParamName.ShouldBe("game");
            }

            [Fact]
            public void Should_throw_with_null_condition()
            {
                // arrange
                var builder = new TestGameBuilder().Compile();
                var game = builder.Game;

                // act
                var actual = Should.Throw<ArgumentNullException>(() => new GameEngine(game, null));

                // assert
                actual.ParamName.ShouldBe("gamePhaseRoot");
            }
        }
    }
}