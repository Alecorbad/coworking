using Coworking.Api.Common;
using Coworking.Api.Domain;
using Coworking.Api.Features.Bookings;

namespace Coworking.Api.UnitTests.Features;

public sealed class BookingRulesTests
{
    private static readonly TimeZoneInfo Rome = TimeZoneInfo.FindSystemTimeZoneById("Europe/Rome");

    // Lunedì 12 ottobre 2026: a Roma è in vigore l'ora legale, UTC+2.
    private static readonly DateOnly Day = new(2026, 10, 12);

    private static readonly Room Alfa = new()
    {
        Id = 1,
        Name = "Sala Alfa",

        Capacity = 4,
        OpeningTime = new(8, 0),
        ClosingTime = new(20, 0),
    };

    [Theory]
    [InlineData(9, 10, 10, 11, false)] // bordi che si toccano: entrambe valide
    [InlineData(10, 11, 9, 10, false)]
    [InlineData(9, 11, 10, 12, true)]
    [InlineData(9, 12, 10, 11, true)] // una dentro l'altra
    [InlineData(9, 10, 9, 10, true)]
    [InlineData(9, 10, 11, 12, false)]
    public void Overlaps_TreatsIntervalsAsHalfOpen(int aStart, int aEnd, int bStart, int bEnd, bool expected)
    {
        Assert.Equal(expected, BookingRules.Overlaps(At(aStart), At(aEnd), At(bStart), At(bEnd)));
    }

    [Fact]
    public void CheckTimeRange_AcceptsHalfHourSteps()
    {
        Assert.Null(BookingRules.CheckTimeRange(At(9, 30), At(10), Rome));
    }

    [Theory]
    [InlineData(10, 0, 10, 0)]
    [InlineData(11, 0, 10, 0)]
    [InlineData(9, 15, 10, 0)]
    [InlineData(9, 0, 9, 45)]
    public void CheckTimeRange_RejectsEmptyReversedOrOffGridRanges(int startH, int startM, int endH, int endM)
    {
        Assert.Equal("INVALID_TIME_RANGE", BookingRules.CheckTimeRange(At(startH, startM), At(endH, endM), Rome)?.Code);
    }

    [Fact]
    public void CheckNotInPast_AcceptsAStartEqualToNow_AndRejectsAnEarlierOne()
    {
        Assert.Null(BookingRules.CheckNotInPast(At(9), now: At(9)));
        Assert.Equal("STARTS_IN_PAST", BookingRules.CheckNotInPast(At(9), now: At(9, 1))?.Code);
    }

    [Theory]
    [InlineData(8, 9)]
    [InlineData(19, 20)]
    [InlineData(8, 20)]
    public void CheckOpeningHours_AcceptsRangesInsideTheRoomHours(int start, int end)
    {
        Assert.Null(BookingRules.CheckOpeningHours(At(start), At(end), Alfa, Rome));
    }

    [Theory]
    [InlineData(7, 9)]
    [InlineData(19, 21)]
    public void CheckOpeningHours_RejectsRangesOutsideTheRoomHours(int start, int end)
    {
        Assert.Equal("OUTSIDE_OPENING_HOURS", BookingRules.CheckOpeningHours(At(start), At(end), Alfa, Rome)?.Code);
    }

    [Fact]
    public void CheckOpeningHours_ReadsTheHoursInTheCoworkingTimeZone()
    {
        // 06:00 UTC sono le 08:00 a Roma: dentro gli orari, anche se in UTC sembrerebbe prima dell'apertura.
        var start = new DateTimeOffset(Day.ToDateTime(new TimeOnly(6, 0)), TimeSpan.Zero);

        Assert.Null(BookingRules.CheckOpeningHours(start, start.AddHours(1), Alfa, Rome));
    }

    [Fact]
    public void CheckOpeningHours_RejectsRangesThatCrossMidnight()
    {
        var room = new Room
        {
            Id = 9,
            Name = "Sala Notte",

            Capacity = 4,
            OpeningTime = new(0, 0),
            ClosingTime = new(23, 30),
        };

        var error = BookingRules.CheckOpeningHours(At(23), At(23).AddHours(2), room, Rome);

        Assert.Equal("OUTSIDE_OPENING_HOURS", error?.Code);
    }

    [Fact]
    public void CheckCapacity_AcceptsUpToTheCapacity_AndRejectsMore()
    {
        Assert.Null(BookingRules.CheckCapacity(4, Alfa));
        Assert.Equal("CAPACITY_EXCEEDED", BookingRules.CheckCapacity(5, Alfa)?.Code);
    }

    [Fact]
    public void CheckRoomActive_RejectsAnInactiveRoomWithAConflict()
    {
        var inactive = new Room
        {
            Id = 5,
            Name = "Sala Epsilon",

            Capacity = 8,
            OpeningTime = new(9, 0),
            ClosingTime = new(13, 0),
            IsActive = false,
        };

        var error = BookingRules.CheckRoomActive(inactive);

        Assert.Equal(("ROOM_INACTIVE", ErrorKind.Conflict), (error?.Code, error?.Kind));
    }

    [Fact]
    public void CheckReadRange_AcceptsUpTo31Days_AndRejectsLongerOrReversedRanges()
    {
        Assert.Null(BookingRules.CheckReadRange(At(0), At(0).AddDays(31)));
        Assert.Equal("INVALID_TIME_RANGE", BookingRules.CheckReadRange(At(0), At(0).AddDays(32))?.Code);
        Assert.Equal("INVALID_TIME_RANGE", BookingRules.CheckReadRange(At(10), At(9))?.Code);
    }

    /// <summary>Ora di Roma del giorno di prova, come istante.</summary>
    private static DateTimeOffset At(int hour, int minute = 0)
    {
        var utc = TimeZoneInfo.ConvertTimeToUtc(Day.ToDateTime(new TimeOnly(hour, minute)), Rome);
        return new DateTimeOffset(utc, TimeSpan.Zero);
    }
}
