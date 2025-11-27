using System;

using Veggerby.Boards.Monopoly.States;

namespace Veggerby.Boards.Monopoly;

/// <summary>
/// Calculates rent for Monopoly properties.
/// </summary>
public static class RentCalculator
{
    /// <summary>
    /// Calculates the rent owed for landing on a property.
    /// </summary>
    /// <param name="squareInfo">The property square information.</param>
    /// <param name="ownerId">The owner player ID.</param>
    /// <param name="ownership">The property ownership state.</param>
    /// <param name="diceRoll">The dice roll total (for utility rent).</param>
    /// <returns>The rent amount.</returns>
    public static int CalculateRent(
        MonopolySquareInfo squareInfo,
        string ownerId,
        PropertyOwnershipState ownership,
        int diceRoll = 0)
    {
        ArgumentNullException.ThrowIfNull(squareInfo);
        ArgumentNullException.ThrowIfNull(ownerId);
        ArgumentNullException.ThrowIfNull(ownership);

        // No rent for mortgaged properties
        if (ownership.IsMortgaged(squareInfo.Position))
        {
            return 0;
        }

        return squareInfo.SquareType switch
        {
            SquareType.Property => CalculatePropertyRent(squareInfo, ownerId, ownership),
            SquareType.Railroad => CalculateRailroadRent(squareInfo, ownerId, ownership),
            SquareType.Utility => CalculateUtilityRent(ownerId, ownership, diceRoll),
            _ => 0
        };
    }

    private static int CalculatePropertyRent(
        MonopolySquareInfo property,
        string ownerId,
        PropertyOwnershipState ownership)
    {
        var houseCount = ownership.GetHouseCount(property.Position);

        // If property has houses/hotel, use the rent schedule
        if (houseCount > 0)
        {
            return property.GetRentForHouseCount(houseCount);
        }

        var baseRent = property.BaseRent;

        // Check monopoly: owner owns all properties in color group
        if (ownership.HasMonopoly(ownerId, property.ColorGroup))
        {
            // Double rent for monopoly (without houses)
            return baseRent * 2;
        }

        return baseRent;
    }

    private static int CalculateRailroadRent(MonopolySquareInfo railroad, string ownerId, PropertyOwnershipState ownership)
    {
        // Count unmortgaged railroads owned
        var railroadsOwned = ownership.CountUnmortgagedInColorGroup(ownerId, PropertyColorGroup.Railroad);

        // $25 for 1, $50 for 2, $100 for 3, $200 for 4 (25 * 2^(n-1))
        return railroadsOwned > 0 ? 25 * (1 << (railroadsOwned - 1)) : 0;
    }

    private static int CalculateUtilityRent(string ownerId, PropertyOwnershipState ownership, int diceRoll)
    {
        // Count unmortgaged utilities owned
        var utilitiesOwned = ownership.CountUnmortgagedInColorGroup(ownerId, PropertyColorGroup.Utility);

        // 4x dice roll for 1 utility, 10x for 2 utilities
        var multiplier = utilitiesOwned switch
        {
            1 => 4,
            2 => 10,
            _ => 0
        };

        return diceRoll * multiplier;
    }
}
