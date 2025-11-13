namespace Veggerby.Boards.Tests.DeckBuilding;

/// <summary>
/// Disables parallelization for deck-building tests to eliminate cross-test feature flag / shared state timing effects.
/// </summary>
[CollectionDefinition(Name, DisableParallelization = true)]
public class DeckBuildingSequentialCollectionDefinition
{
    public const string Name = "DeckBuildingSequential";
}