using System;
using System.Collections.Generic;

using Veggerby.Boards.Risk.Conditions;
using Veggerby.Boards.Risk.Events;
using Veggerby.Boards.Risk.Mutators;
using Veggerby.Boards.States.Conditions;

namespace Veggerby.Boards.Risk;

/// <summary>
/// Concrete <see cref="GameBuilder"/> defining a Risk-style territory conquest game.
/// </summary>
/// <remarks>
/// This builder creates a simplified world map with 24 territories across 4 continents.
/// The game demonstrates multi-dice combat resolution patterns and territory control mechanics.
/// </remarks>
public class RiskGameBuilder : GameBuilder
{
    /// <summary>
    /// Continent definitions for the simplified map.
    /// </summary>
    private readonly List<Continent> _continents = new();

    /// <summary>
    /// Direction for territory adjacency.
    /// </summary>
    public const string AdjacentDirection = "adjacent";

    /// <summary>
    /// Gets the configured continents.
    /// </summary>
    public IReadOnlyList<Continent> Continents => _continents;

    /// <summary>
    /// Configures the Risk board with territories, continents, players, and rules.
    /// </summary>
    protected override void Build()
    {
        BoardId = "risk";

        // Players (simplified 2-player version)
        AddPlayer(RiskIds.Players.Red);
        AddPlayer(RiskIds.Players.Blue);

        // Initial active player
        WithActivePlayer(RiskIds.Players.Red, true);
        WithActivePlayer(RiskIds.Players.Blue, false);

        // Direction for adjacency
        AddDirection(AdjacentDirection);

        // Build simplified world map
        BuildSimplifiedWorldMap();

        // Configure game phases
        ConfigureGamePhases();

        // Set initial state
        ConfigureInitialState();
    }

    /// <summary>
    /// Builds a simplified world map with 24 territories in 4 continents.
    /// </summary>
    /// <remarks>
    /// Continents and territories:
    /// - North (6 territories, bonus 3): Alaska, NorthWest, GreenLand, Alberta, Ontario, Quebec
    /// - South (4 territories, bonus 2): Venezuela, Peru, Brazil, Argentina
    /// - Europe (7 territories, bonus 5): Iceland, Britain, Scandinavia, WestEurope, NorthEurope, SouthEurope, Ukraine
    /// - Asia (7 territories, bonus 7): Ural, Siberia, Mongolia, China, India, MiddleEast, Japan
    /// </remarks>
    private void BuildSimplifiedWorldMap()
    {
        // North Continent (6 territories, 3 bonus)
        AddTile(RiskIds.Territories.Alaska);
        AddTile(RiskIds.Territories.NorthWest);
        AddTile(RiskIds.Territories.GreenLand);
        AddTile(RiskIds.Territories.Alberta);
        AddTile(RiskIds.Territories.Ontario);
        AddTile(RiskIds.Territories.Quebec);

        // North adjacencies
        WithTile(RiskIds.Territories.Alaska).WithRelationTo(RiskIds.Territories.NorthWest).InDirection(AdjacentDirection);
        WithTile(RiskIds.Territories.Alaska).WithRelationTo(RiskIds.Territories.Alberta).InDirection(AdjacentDirection);
        WithTile(RiskIds.Territories.NorthWest).WithRelationTo(RiskIds.Territories.Alberta).InDirection(AdjacentDirection);
        WithTile(RiskIds.Territories.NorthWest).WithRelationTo(RiskIds.Territories.GreenLand).InDirection(AdjacentDirection);
        WithTile(RiskIds.Territories.NorthWest).WithRelationTo(RiskIds.Territories.Ontario).InDirection(AdjacentDirection);
        WithTile(RiskIds.Territories.Alberta).WithRelationTo(RiskIds.Territories.Ontario).InDirection(AdjacentDirection);
        WithTile(RiskIds.Territories.GreenLand).WithRelationTo(RiskIds.Territories.Ontario).InDirection(AdjacentDirection);
        WithTile(RiskIds.Territories.GreenLand).WithRelationTo(RiskIds.Territories.Quebec).InDirection(AdjacentDirection);
        WithTile(RiskIds.Territories.Ontario).WithRelationTo(RiskIds.Territories.Quebec).InDirection(AdjacentDirection);

        _continents.Add(new Continent(
            RiskIds.Continents.North,
            "North America",
            3,
            new[]
            {
                RiskIds.Territories.Alaska,
                RiskIds.Territories.NorthWest,
                RiskIds.Territories.GreenLand,
                RiskIds.Territories.Alberta,
                RiskIds.Territories.Ontario,
                RiskIds.Territories.Quebec
            }));

        // South Continent (4 territories, 2 bonus)
        AddTile(RiskIds.Territories.Venezuela);
        AddTile(RiskIds.Territories.Peru);
        AddTile(RiskIds.Territories.Brazil);
        AddTile(RiskIds.Territories.Argentina);

        // South adjacencies
        WithTile(RiskIds.Territories.Venezuela).WithRelationTo(RiskIds.Territories.Peru).InDirection(AdjacentDirection);
        WithTile(RiskIds.Territories.Venezuela).WithRelationTo(RiskIds.Territories.Brazil).InDirection(AdjacentDirection);
        WithTile(RiskIds.Territories.Peru).WithRelationTo(RiskIds.Territories.Brazil).InDirection(AdjacentDirection);
        WithTile(RiskIds.Territories.Peru).WithRelationTo(RiskIds.Territories.Argentina).InDirection(AdjacentDirection);
        WithTile(RiskIds.Territories.Brazil).WithRelationTo(RiskIds.Territories.Argentina).InDirection(AdjacentDirection);

        _continents.Add(new Continent(
            RiskIds.Continents.South,
            "South America",
            2,
            new[]
            {
                RiskIds.Territories.Venezuela,
                RiskIds.Territories.Peru,
                RiskIds.Territories.Brazil,
                RiskIds.Territories.Argentina
            }));

        // Europe Continent (7 territories, 5 bonus)
        AddTile(RiskIds.Territories.Iceland);
        AddTile(RiskIds.Territories.Britain);
        AddTile(RiskIds.Territories.Scandinavia);
        AddTile(RiskIds.Territories.WestEurope);
        AddTile(RiskIds.Territories.NorthEurope);
        AddTile(RiskIds.Territories.SouthEurope);
        AddTile(RiskIds.Territories.Ukraine);

        // Europe adjacencies
        WithTile(RiskIds.Territories.Iceland).WithRelationTo(RiskIds.Territories.Britain).InDirection(AdjacentDirection);
        WithTile(RiskIds.Territories.Iceland).WithRelationTo(RiskIds.Territories.Scandinavia).InDirection(AdjacentDirection);
        WithTile(RiskIds.Territories.Britain).WithRelationTo(RiskIds.Territories.WestEurope).InDirection(AdjacentDirection);
        WithTile(RiskIds.Territories.Britain).WithRelationTo(RiskIds.Territories.NorthEurope).InDirection(AdjacentDirection);
        WithTile(RiskIds.Territories.Britain).WithRelationTo(RiskIds.Territories.Scandinavia).InDirection(AdjacentDirection);
        WithTile(RiskIds.Territories.Scandinavia).WithRelationTo(RiskIds.Territories.NorthEurope).InDirection(AdjacentDirection);
        WithTile(RiskIds.Territories.Scandinavia).WithRelationTo(RiskIds.Territories.Ukraine).InDirection(AdjacentDirection);
        WithTile(RiskIds.Territories.WestEurope).WithRelationTo(RiskIds.Territories.NorthEurope).InDirection(AdjacentDirection);
        WithTile(RiskIds.Territories.WestEurope).WithRelationTo(RiskIds.Territories.SouthEurope).InDirection(AdjacentDirection);
        WithTile(RiskIds.Territories.NorthEurope).WithRelationTo(RiskIds.Territories.SouthEurope).InDirection(AdjacentDirection);
        WithTile(RiskIds.Territories.NorthEurope).WithRelationTo(RiskIds.Territories.Ukraine).InDirection(AdjacentDirection);
        WithTile(RiskIds.Territories.SouthEurope).WithRelationTo(RiskIds.Territories.Ukraine).InDirection(AdjacentDirection);

        _continents.Add(new Continent(
            RiskIds.Continents.Europe,
            "Europe",
            5,
            new[]
            {
                RiskIds.Territories.Iceland,
                RiskIds.Territories.Britain,
                RiskIds.Territories.Scandinavia,
                RiskIds.Territories.WestEurope,
                RiskIds.Territories.NorthEurope,
                RiskIds.Territories.SouthEurope,
                RiskIds.Territories.Ukraine
            }));

        // Asia Continent (7 territories, 7 bonus)
        AddTile(RiskIds.Territories.Ural);
        AddTile(RiskIds.Territories.Siberia);
        AddTile(RiskIds.Territories.Mongolia);
        AddTile(RiskIds.Territories.China);
        AddTile(RiskIds.Territories.India);
        AddTile(RiskIds.Territories.MiddleEast);
        AddTile(RiskIds.Territories.Japan);

        // Asia adjacencies
        WithTile(RiskIds.Territories.Ural).WithRelationTo(RiskIds.Territories.Siberia).InDirection(AdjacentDirection);
        WithTile(RiskIds.Territories.Ural).WithRelationTo(RiskIds.Territories.China).InDirection(AdjacentDirection);
        WithTile(RiskIds.Territories.Siberia).WithRelationTo(RiskIds.Territories.Mongolia).InDirection(AdjacentDirection);
        WithTile(RiskIds.Territories.Siberia).WithRelationTo(RiskIds.Territories.China).InDirection(AdjacentDirection);
        WithTile(RiskIds.Territories.Mongolia).WithRelationTo(RiskIds.Territories.China).InDirection(AdjacentDirection);
        WithTile(RiskIds.Territories.Mongolia).WithRelationTo(RiskIds.Territories.Japan).InDirection(AdjacentDirection);
        WithTile(RiskIds.Territories.China).WithRelationTo(RiskIds.Territories.India).InDirection(AdjacentDirection);
        WithTile(RiskIds.Territories.India).WithRelationTo(RiskIds.Territories.MiddleEast).InDirection(AdjacentDirection);

        _continents.Add(new Continent(
            RiskIds.Continents.Asia,
            "Asia",
            7,
            new[]
            {
                RiskIds.Territories.Ural,
                RiskIds.Territories.Siberia,
                RiskIds.Territories.Mongolia,
                RiskIds.Territories.China,
                RiskIds.Territories.India,
                RiskIds.Territories.MiddleEast,
                RiskIds.Territories.Japan
            }));

        // Cross-continent connections
        // North America -> Europe
        WithTile(RiskIds.Territories.GreenLand).WithRelationTo(RiskIds.Territories.Iceland).InDirection(AdjacentDirection);
        // North America -> South America
        WithTile(RiskIds.Territories.Quebec).WithRelationTo(RiskIds.Territories.Venezuela).InDirection(AdjacentDirection);
        // South America -> Europe
        WithTile(RiskIds.Territories.Brazil).WithRelationTo(RiskIds.Territories.WestEurope).InDirection(AdjacentDirection);
        // Europe -> Asia
        WithTile(RiskIds.Territories.Ukraine).WithRelationTo(RiskIds.Territories.Ural).InDirection(AdjacentDirection);
        WithTile(RiskIds.Territories.SouthEurope).WithRelationTo(RiskIds.Territories.MiddleEast).InDirection(AdjacentDirection);
        // North America -> Asia (Alaska-Siberia sea route)
        WithTile(RiskIds.Territories.Alaska).WithRelationTo(RiskIds.Territories.Siberia).InDirection(AdjacentDirection);
    }

    /// <summary>
    /// Configures the game phases: Reinforce, Attack, Fortify.
    /// </summary>
    private void ConfigureGamePhases()
    {
        AddGamePhase("risk-game")
            .WithEndGameDetection(
                game => new WorldDominationCondition(),
                game => new RiskEndGameMutator(game))
            .If<GameNotEndedCondition>()
            .Then()
                // Reinforcement phase
                .ForEvent<PlaceArmiesGameEvent>()
                    .If<PlaceArmiesCondition>()
                .Then()
                    .Do<PlaceArmiesStateMutator>()
                // Attack phase
                .ForEvent<AttackGameEvent>()
                    .If(game => new AttackCondition(game))
                .Then()
                    .Do<CombatResolutionStateMutator>()
                // Conquest (after defender eliminated)
                .ForEvent<ConquerTerritoryGameEvent>()
                    .If<ConquerCondition>()
                .Then()
                    .Do<ConquerTerritoryStateMutator>()
                // End Attack phase
                .ForEvent<EndAttackPhaseGameEvent>()
                .Then()
                    .Do<EndAttackPhaseStateMutator>()
                // Fortify phase
                .ForEvent<FortifyGameEvent>()
                    .If(game => new FortifyCondition(game))
                .Then()
                    .Do<FortifyStateMutator>()
                // End Fortify (ends turn)
                .ForEvent<EndFortifyPhaseGameEvent>()
                .Then()
                    .Do(game => new EndFortifyPhaseStateMutator(_continents))
                // Skip Fortify (ends turn without fortification)
                .ForEvent<SkipFortifyPhaseGameEvent>()
                .Then()
                    .Do(game => new SkipFortifyPhaseStateMutator(_continents));
    }

    /// <summary>
    /// Configures initial territory ownership and armies.
    /// </summary>
    /// <remarks>
    /// Default distribution: alternating ownership, 3 armies per territory.
    /// </remarks>
    private void ConfigureInitialState()
    {
        // Calculate reinforcements for starting player (Red)
        // With 12 territories: 12/3 = 4 (meets minimum 3)
        var startingReinforcements = 3; // Start with minimum for demo

        // Store Risk-specific state
        WithState(new RiskStateExtras(
            Continents: _continents,
            CurrentPhase: RiskPhase.Reinforce,
            ReinforcementsRemaining: startingReinforcements,
            ConqueredThisTurn: false,
            MinimumConquestArmies: null));
    }
}
