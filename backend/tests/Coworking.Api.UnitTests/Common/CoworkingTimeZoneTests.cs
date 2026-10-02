using Coworking.Api.Common;

namespace Coworking.Api.UnitTests.Common;

public sealed class CoworkingTimeZoneTests
{
    [Fact]
    public void FromId_ReturnsTimeZone_WhenIdIsIana()
    {
        var timeZone = CoworkingTimeZone.FromId("Europe/Rome");

        Assert.Equal("Europe/Rome", timeZone.Id);
    }

    [Fact]
    public void FromId_Throws_WhenIdIsUnknown()
    {
        Assert.Throws<InvalidOperationException>(() => CoworkingTimeZone.FromId("Europe/Atlantide"));
    }
}
