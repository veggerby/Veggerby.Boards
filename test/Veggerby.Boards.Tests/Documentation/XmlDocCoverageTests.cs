using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

using AwesomeAssertions;

using Veggerby.Boards.Diagnostics;

using Xunit;

namespace Veggerby.Boards.Tests.Documentation;

/// <summary>
/// Ensures public types in the core assembly have XML documentation entries unless explicitly ignored.
/// </summary>
public class XmlDocCoverageTests
{
    [Fact]
    public void GivenPublicTypes_WhenEnumerated_ThenXmlDocsExistForEachUnlessIgnored()
    {
        // arrange
        var assembly = typeof(Veggerby.Boards.GameBuilder).Assembly; // anchor on core assembly
        var xmlPath = Path.Combine(Path.GetDirectoryName(assembly.Location)!, Path.GetFileNameWithoutExtension(assembly.Location) + ".xml");
        File.Exists(xmlPath).Should().BeTrue("XML documentation file must be present for coverage enforcement");
        var docMembers = LoadTypeMembers(xmlPath);

        var publicTypes = assembly.GetExportedTypes()
            .Where(t => !t.IsNested)
            .Where(t => !t.GetCustomAttributes(typeof(DocCoverageIgnoreAttribute), inherit: false).Any())
            .ToList();

        // act
        var undocumented = publicTypes
            .Where(t => !docMembers.Contains("T:" + t.FullName))
            .ToList();

        // assert
        undocumented.Should().BeEmpty("All public types must have XML docs: {0}", string.Join(", ", undocumented.Select(x => x.FullName)));
    }

    private static HashSet<string> LoadTypeMembers(string xmlPath)
    {
        var set = new HashSet<string>(StringComparer.Ordinal);
        var doc = XDocument.Load(xmlPath);
        foreach (var member in doc.Descendants("member"))
        {
            var nameAttr = member.Attribute("name");
            if (nameAttr is null)
            {
                continue;
            }
            if (nameAttr.Value.StartsWith("T:", StringComparison.Ordinal))
            {
                set.Add(nameAttr.Value);
            }
        }
        return set;
    }
}
