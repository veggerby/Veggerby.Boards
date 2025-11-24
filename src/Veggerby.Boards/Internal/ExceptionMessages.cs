using System;

namespace Veggerby.Boards.Internal;

/// <summary>
/// Central repository of standardized exception message text to ensure consistency across modules.
/// </summary>
/// <remarks>
/// Messages are concise and context-neutral; callers may append artifact identifiers for specificity.
/// Not a localization surface; considered stable only for internal diagnostic clarity.
/// </remarks>
public static class ExceptionMessages
{
    /// <summary>Thrown when one or more required deck piles are absent.</summary>
    public const string RequiredPilesMissing = "Required piles missing";

    /// <summary>Thrown when the Hand pile is absent in a deck state.</summary>
    public const string HandPileMissing = "Hand pile missing";

    /// <summary>Thrown when a composite policy is constructed without at least one inner policy.</summary>
    public const string AtLeastOnePolicyRequired = "At least one policy required";

    /// <summary>Thrown when a composite mutator or condition definition is created without at least one child.</summary>
    public const string AtLeastOneChildRequired = "At least one child mutator or condition required";

    /// <summary>Thrown when multiple conditions are added without specifying a composite mode.</summary>
    public const string CompositeModeRequired = "Composite mode required for multiple conditions";

    /// <summary>Thrown when attempting to mix different composite modes.</summary>
    public const string CompositeModeMismatch = "Composite mode mismatch";

    /// <summary>Thrown when at least one direction must be specified for a piece pattern.</summary>
    public const string AtLeastOneDirectionRequired = "At least one direction required";

    /// <summary>Thrown when all directions must be non-null and non-empty.</summary>
    public const string DirectionsInvalid = "All directions must be non-null and non-empty";

    /// <summary>Thrown when a game event rule set composite mode is set after rules were added.</summary>
    public const string CompositeModeSetAfterRules = "Composite mode can only be set before adding rules";

    /// <summary>Thrown when a turn profile contains duplicate segment identifiers.</summary>
    public const string DuplicateTurnProfileSegment = "Duplicate segment in turn profile";

    /// <summary>Thrown when a turn profile has no segments.</summary>
    public const string TurnProfileEmpty = "Turn profile must contain at least one segment";

    /// <summary>Thrown when dice list empty for a dice condition.</summary>
    public const string DiceListEmpty = "Dice list cannot be empty";

    /// <summary>Thrown when dice list contains null values.</summary>
    public const string DiceListContainsNull = "All dice must be non null";

    /// <summary>Thrown when path compilation provided empty fixed steps.</summary>
    public const string EmptyFixedSteps = "Empty fixed steps";

    /// <summary>Thrown when path compilation provided empty multi-ray directions.</summary>
    public const string EmptyMultiRayDirections = "Empty multi-ray directions";

    /// <summary>Thrown when clearing dice requires at least one dice candidate.</summary>
    public const string AtLeastOneDiceRequired = "At least one dice must be added to condition";

    /// <summary>Thrown when board exceeds 64 tiles for a 64-bit bitboard layout.</summary>
    public const string BoardTooLargeForBitboard = "Bitboard layout only supports boards with <=64 tiles.";

    /// <summary>Thrown when artifact state change provided mismatched artifacts.</summary>
    public const string ArtifactStateChangeMismatch = "To and From need to reference the same artifact";

    /// <summary>Thrown when roll dice game event constructed without at least one state.</summary>
    public const string AtLeastOneDiceStateRequired = "Must provide at least one new state";

    /// <summary>Creates a message indicating a specific pile id is missing.</summary>
    /// <param name="pileId">The missing pile identifier.</param>
    /// <returns>Formatted exception message.</returns>
    public static string MissingPile(string pileId) => $"Missing pile '{pileId}'";

    /// <summary>Creates a message indicating a specified card id was not found among game artifacts.</summary>
    /// <param name="cardId">The card identifier.</param>
    /// <returns>Formatted exception message.</returns>
    public static string CardNotFound(string cardId) => $"Card '{cardId}' not found in game artifacts.";
}
