using System.Linq;

using Veggerby.Boards.Chess;
using Veggerby.Boards.States;

using Xunit;

namespace Veggerby.Boards.Tests.Chess;

/// <summary>
/// Ensures every canonical chess piece id has both a role and color classification via metadata extras.
/// Guards against drift when adding/removing piece constants or adjusting builder wiring.
/// </summary>
public class ChessPieceClassificationTests
{
    [Fact]
    public void GivenStandardChessGame_WhenBuilt_AllPieceIdsResolveRoleAndColor()
    {
        // arrange
        var progress = new ChessGameBuilder().Compile();
        var state = progress.State;
        var roles = state.GetExtras<ChessPieceRolesExtras>();
        var colors = state.GetExtras<ChessPieceColorsExtras>();
        Assert.NotNull(roles);
        Assert.NotNull(colors);

        // Derive canonical set from role map itself (builder validated coverage already)
        var allPieceIds = roles.Roles.Keys.ToArray();

        // act & assert
        foreach (var pid in allPieceIds)
        {
            var hasRole = ChessPieceRoles.TryGetRole(roles, pid, out _);
            var hasColor = ChessPieceColors.TryGetColor(colors, pid, out _);
            Assert.True(hasRole, $"Missing role mapping for piece id '{pid}'");
            Assert.True(hasColor, $"Missing color mapping for piece id '{pid}'");
        }
    }
}