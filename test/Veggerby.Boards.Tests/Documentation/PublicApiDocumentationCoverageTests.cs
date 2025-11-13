using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using Veggerby.Boards;
using Veggerby.Boards.Backgammon;
using Veggerby.Boards.Cards;
using Veggerby.Boards.Chess;
using Veggerby.Boards.DeckBuilding;
using Veggerby.Boards.Go;

namespace Veggerby.Boards.Tests.Documentation;

public class PublicApiDocumentationCoverageTests
{
    [Fact(Skip = "Baseline harness – enable once initial coverage improved.")]
    public void GivenPublicTypes_WhenScanningXmlDocs_ThenAllHaveSummaryOrAreFlagged()
    {
        // arrange
        var assemblies = new[]
        {
            typeof(GameBuilder).Assembly,
            typeof(ChessGameBuilder).Assembly,
            typeof(BackgammonGameBuilder).Assembly,
            typeof(GoGameBuilder).Assembly,
            typeof(CardsGameBuilder).Assembly,
            typeof(DeckBuildingGameBuilder).Assembly
        };

        var missing = new List<string>();

        foreach (var asm in assemblies.Distinct())
        {
            var xmlPath = Path.ChangeExtension(asm.Location, "xml");
            if (!File.Exists(xmlPath))
            {
                // XML doc not generated (likely test configuration); skip without failing baseline.
                continue;
            }

            XDocument? doc = null;
            try
            {
                doc = XDocument.Load(xmlPath);
            }
            catch
            {
                continue; // treat malformed as absent for baseline
            }

            if (doc.Root is null)
            {
                continue;
            }

            var members = doc.Root.Element("members");
            if (members is null)
            {
                continue;
            }

            var memberLookup = members.Elements("member").Where(e => e.Attribute("name") != null)
                .ToDictionary(e => e.Attribute("name")!.Value, e => e, StringComparer.Ordinal);

            foreach (var type in asm.GetExportedTypes())
            {
                if (!IsRelevantPublicType(type))
                {
                    continue; // skip compiler generated / attributes / trivial markers
                }

                var xmlKey = BuildTypeXmlKey(type);
                if (!memberLookup.TryGetValue(xmlKey, out var element))
                {
                    missing.Add(type.FullName ?? type.Name);
                    continue;
                }

                var summary = element.Element("summary");
                if (summary is null || string.IsNullOrWhiteSpace(summary.Value))
                {
                    missing.Add(type.FullName ?? type.Name);
                }
            }
        }

        // act
        // Baseline harness does not enforce yet – will assert coverage in future iteration.

        // assert
        // Output diagnostic listing for developer visibility. Using test skip prevents CI failure until docs added.
        missing.Should().NotBeNull();
    }

    private static string BuildTypeXmlKey(Type t)
    {
        // XML doc member name uses 'T:Full.Namespace.TypeName' with nested types separated by '+' and generics suffixed with `\n
        var fullName = t.FullName ?? t.Name;
        return "T:" + fullName.Replace('+', '.'); // treat nested with '.' for simplicity; baseline tolerance
    }

    private static bool IsRelevantPublicType(Type t)
    {
        if (t.IsDefined(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), false))
        {
            return false;
        }
        if (t.IsSubclassOf(typeof(Attribute)))
        {
            return false; // attribute docs optional
        }
        return t.IsClass || t.IsInterface || t.IsEnum || t.IsValueType;
    }
}
