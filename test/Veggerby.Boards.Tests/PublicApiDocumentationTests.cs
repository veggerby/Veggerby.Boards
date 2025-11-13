using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

using AwesomeAssertions;

using Xunit;

namespace Veggerby.Boards.Tests
{
    public class PublicApiDocumentationTests
    {
        [Fact(Skip = "Redundant: XML doc coverage already enforced by GenerateDocumentationFile + TreatWarningsAsErrors (CS1591). Test retained for potential future semantic validation (e.g., remarks/invariant checks).")]
        public void GivenPublicTypes_WhenScanningDocs_ThenAllHaveSummary()
        {
            // arrange
            var assembly = typeof(Veggerby.Boards.Artifacts.Game).Assembly; // core assembly
            var xmlPath = Path.Combine(Path.GetDirectoryName(assembly.Location)!, "Veggerby.Boards.xml");

            if (!File.Exists(xmlPath))
            {
                throw new InvalidOperationException($"Expected XML documentation file not found: {xmlPath}");
            }

            var doc = XDocument.Load(xmlPath);
            var summaryLookup = doc.Descendants("member")
                .Where(m => m.Attribute("name") != null)
                .ToDictionary(m => m.Attribute("name")!.Value, m => m.Descendants("summary").FirstOrDefault()?.Value?.Trim() ?? string.Empty);

            // Public types (classes, structs, interfaces) exported from assembly
            var publicTypes = assembly.GetExportedTypes()
                .Where(t => t.IsClass || t.IsInterface || t.IsValueType)
                .ToList();

            var offenders = new List<string>();
            foreach (var type in publicTypes)
            {
                var docId = GetTypeDocId(type);
                if (!summaryLookup.TryGetValue(docId, out var summary) || string.IsNullOrWhiteSpace(summary))
                {
                    offenders.Add(type.FullName ?? type.Name);
                }
            }

            // act
            // no act (pure read)

            // assert
            offenders.Should().BeEmpty($"Public types missing <summary>: {string.Join(", ", offenders)}");
        }

        private static string GetTypeDocId(Type t)
        {
            // XML doc id format: T:Namespace.TypeName (nested types use +)
            var name = t.FullName ?? t.Name;
            return $"T:{name.Replace('+', '.')}";
        }
    }
}
