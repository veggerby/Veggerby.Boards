using System;
using System.Linq;
using System.Reflection;

namespace Veggerby.Boards.Tests.Infrastructure;

/// <summary>
/// Guard test ensuring the legacy Utils.FeatureFlagScope type is not reintroduced.
/// This prevents accidental resurrection via merges or cherry-picks after migration.
/// </summary>
public class FeatureFlagScopeLegacyGuardTests
{
    [Fact]
    public void GivenLoadedAssemblies_WhenScanningTypes_ThenLegacyFeatureFlagScopeDoesNotExist()
    {
        // arrange
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        // act
        var legacyType = assemblies
            .SelectMany(a => SafeGetTypes(a))
            .FirstOrDefault(t => t.FullName == "Veggerby.Boards.Tests.Utils.FeatureFlagScope");

        // assert
        legacyType.Should().BeNull("legacy test utility must remain deleted; use Infrastructure.FeatureFlagScope instead");
    }

    private static Type[] SafeGetTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(t => t != null).ToArray()!;
        }
    }
}