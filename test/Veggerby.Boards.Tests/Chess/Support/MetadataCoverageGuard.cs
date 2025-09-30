using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Chess;

namespace Veggerby.Boards.Tests.Chess.Support;

/// <summary>
/// Helper to assert that all pieces added in a scenario builder have matching role & color metadata entries.
/// Fail-fast to surface drift when adding new custom piece ids without updating metadata dictionaries.
/// </summary>
internal static class MetadataCoverageGuard
{
    public static void AssertAllPiecesCovered(GameBuilder builder, IDictionary<string, ChessPieceRole> roles, IDictionary<string, ChessPieceColor> colors)
    {
        // Collect piece ids added so far from builder (artifacts list public via reflection not exposed; rely on dictionaries passed for now)
        // Approach: union of role/color keys must match each dictionary independently; find any asymmetry.
        var roleKeys = roles.Keys.ToHashSet();
        var colorKeys = colors.Keys.ToHashSet();
        if (!roleKeys.SetEquals(colorKeys))
        {
            var missingRole = colorKeys.Except(roleKeys).ToArray();
            var missingColor = roleKeys.Except(colorKeys).ToArray();
            var message = $"Metadata coverage mismatch. Missing role for: [{string.Join(',', missingRole)}]; Missing color for: [{string.Join(',', missingColor)}]";
            throw new System.InvalidOperationException(message);
        }
        // No-op if aligned.
    }
}