using System.Collections.Generic;

namespace Veggerby.Boards.Tests.DeckBuilding;

internal static class DeckBuildingTestHelpers
{
    public static IDictionary<string, int> BuildSupply(params (string id, int count)[] items)
    {
        var dict = new Dictionary<string, int>(System.StringComparer.Ordinal);
        if (items is null)
        {
            return dict;
        }
        for (var i = 0; i < items.Length; i++)
        {
            var (id, count) = items[i];
            if (!string.IsNullOrEmpty(id) && count > 0)
            {
                dict[id] = count;
            }
        }
        return dict;
    }
}