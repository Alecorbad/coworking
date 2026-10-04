using Coworking.Api.Common;

namespace Coworking.Api.UnitTests.Common;

public sealed class ResultTests
{
    [Fact]
    public void Match_CallsValueBranch_WhenResultHoldsAValue()
    {
        Result<int> result = 42;

        var text = result.Match(value => $"ok {value}", error => $"ko {error.Code}");

        Assert.Equal("ok 42", text);
    }

    [Fact]
    public void Match_CallsErrorBranch_WhenResultHoldsAnError()
    {
        Result<int> result = new ApiError(ErrorKind.Conflict, "ROOM_OVERLAP", "The room is already booked.");

        var text = result.Match(value => $"ok {value}", error => $"ko {error.Code}");

        Assert.Equal("ko ROOM_OVERLAP", text);
    }
}
