namespace Veggerby.Boards.Tests.Enforcement;

public class AssertGuardDemoTests
{
    [Fact]
    public void LegacyAssertInvocationShouldTriggerBannedApi()
    {
        // arrange / act / assert (banned symbol usage)
#pragma warning disable RS0030 // Do not use banned APIs
        Assert.True(true);
#pragma warning restore RS0030 // Do not use banned APIs
    }
}