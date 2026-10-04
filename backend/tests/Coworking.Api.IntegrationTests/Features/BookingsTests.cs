using System.Net;
using System.Net.Http.Json;
using Coworking.Api.Domain;
using Coworking.Api.Features.Bookings;
using Coworking.Api.Features.Customers;
using Coworking.Api.Infrastructure;
using Coworking.Api.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Coworking.Api.IntegrationTests.Features;

[Collection(SharedApi.Name)]
public sealed class BookingsTests(ApiFactory factory)
{
    private const int Alfa = 1; // capienza 4, 08:00-20:00
    private const int Beta = 2; // capienza 6, 08:00-20:00
    private const int Epsilon = 5; // disattivata nel seed

    private static readonly CancellationToken Ct = TestContext.Current.CancellationToken;
    private static readonly TimeZoneInfo Rome = TimeZoneInfo.FindSystemTimeZoneById("Europe/Rome");
    private static int nextDay;

    [Fact]
    public async Task Create_Returns201WithLocation_AndTheNamesOfRoomAndCustomer()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var customer = await CreateCustomerAsync(client);
        var day = UniqueDay();

        var response = await client.PostAsJsonAsync("/api/bookings", Request(customer.Id, Alfa, day, 9, 10), Ct);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<BookingResponse>(Ct);
        Assert.NotNull(created);
        Assert.Equal((BookingStatus.Confirmed, "Sala Alfa", $"{customer.FirstName} {customer.LastName}"),
            (created.Status, created.RoomName, created.CustomerName));
        Assert.Equal(At(day, 9), created.StartsAt);
        var read = await client.GetFromJsonAsync<BookingResponse>(response.Headers.Location, Ct);
        Assert.Equal(created, read);
    }

    [Fact]
    public async Task Create_AcceptsBookingsThatOnlyTouchAtTheBorder()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var (first, second) = (await CreateCustomerAsync(client), await CreateCustomerAsync(client));
        var day = UniqueDay();

        var early = await client.PostAsJsonAsync("/api/bookings", Request(first.Id, Alfa, day, 9, 10), Ct);
        var late = await client.PostAsJsonAsync("/api/bookings", Request(second.Id, Alfa, day, 10, 11), Ct);

        Assert.Equal((HttpStatusCode.Created, HttpStatusCode.Created), (early.StatusCode, late.StatusCode));
    }

    [Fact]
    public async Task Create_Returns409RoomOverlap_WhenTheRoomIsTaken()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var (first, second) = (await CreateCustomerAsync(client), await CreateCustomerAsync(client));
        var day = UniqueDay();
        await CreateBookingAsync(client, Request(first.Id, Alfa, day, 9, 11));

        var response = await client.PostAsJsonAsync("/api/bookings", Request(second.Id, Alfa, day, 10, 12), Ct);

        await ProblemAssert.IsProblemAsync(response, HttpStatusCode.Conflict, "ROOM_OVERLAP");
    }

    [Fact]
    public async Task Create_Returns409CustomerOverlap_WhenTheCustomerIsBusyInAnotherRoom()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var customer = await CreateCustomerAsync(client);
        var day = UniqueDay();
        await CreateBookingAsync(client, Request(customer.Id, Alfa, day, 9, 11));

        var response = await client.PostAsJsonAsync("/api/bookings", Request(customer.Id, Beta, day, 10, 12), Ct);

        await ProblemAssert.IsProblemAsync(response, HttpStatusCode.Conflict, "CUSTOMER_OVERLAP");
    }

    [Fact]
    public async Task Create_ReturnsOne201AndOnly409RoomOverlap_WhenRequestsForTheSameSlotArriveTogether()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var customers = await Task.WhenAll(Enumerable.Range(0, 6).Select(_ => CreateCustomerAsync(client)));
        var day = UniqueDay();

        var responses = await Task.WhenAll(customers.Select(c =>
            client.PostAsJsonAsync("/api/bookings", Request(c.Id, Alfa, day, 9, 10), Ct)));

        Assert.Single(responses, r => r.StatusCode == HttpStatusCode.Created);
        foreach (var response in responses.Where(r => r.StatusCode != HttpStatusCode.Created))
        {
            await ProblemAssert.IsProblemAsync(response, HttpStatusCode.Conflict, "ROOM_OVERLAP");
        }
    }

    [Theory]
    [InlineData(true, BookingConfiguration.RoomOverlapConstraint)]
    [InlineData(false, BookingConfiguration.CustomerOverlapConstraint)]
    public async Task Database_RejectsOverlaps_EvenWithoutTheServiceCheck(bool sameRoom, string constraint)
    {
        // Il controllo del service arriva quasi sempre primo: qui si scrive direttamente, come farebbero
        // due richieste passate insieme dal controllo, e deve fermarle il vincolo EXCLUDE.
        var client = await factory.CreateAuthenticatedClientAsync();
        var (first, second) = (await CreateCustomerAsync(client), await CreateCustomerAsync(client));
        var day = UniqueDay();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Booking Row(int customerId, int roomId) => new()
        {
            CustomerId = customerId,
            RoomId = roomId,
            StartsAt = At(day, 9),
            EndsAt = At(day, 10),
            Attendees = 2,
            CreatedAt = DateTimeOffset.UtcNow,
        };
        db.Bookings.Add(Row(first.Id, Alfa));
        await db.SaveChangesAsync(Ct);

        db.Bookings.Add(sameRoom ? Row(second.Id, Alfa) : Row(first.Id, Beta));
        var exception = await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync(Ct));

        Assert.True(exception.IsViolationOf(constraint));
    }

    [Fact]
    public async Task Create_IgnoresCancelledBookings()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var customer = await CreateCustomerAsync(client);
        var day = UniqueDay();
        var cancelled = await CreateBookingAsync(client, Request(customer.Id, Alfa, day, 9, 10));
        (await client.PostAsync($"/api/bookings/{cancelled.Id}/cancel", null, Ct)).EnsureSuccessStatusCode();

        var response = await client.PostAsJsonAsync("/api/bookings", Request(customer.Id, Alfa, day, 9, 10), Ct);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Theory]
    [InlineData(10, 0, 9, 0, 2, "INVALID_TIME_RANGE")]
    [InlineData(9, 15, 10, 0, 2, "INVALID_TIME_RANGE")]
    [InlineData(7, 0, 9, 0, 2, "OUTSIDE_OPENING_HOURS")]
    [InlineData(19, 0, 21, 0, 2, "OUTSIDE_OPENING_HOURS")]
    [InlineData(9, 0, 10, 0, 5, "CAPACITY_EXCEEDED")]
    public async Task Create_Returns400_WhenTheRequestBreaksARule(
        int startH, int startM, int endH, int endM, int attendees, string code)
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var day = UniqueDay();
        var request = new CreateBookingRequest(1, Alfa, At(day, startH, startM), At(day, endH, endM), attendees);

        var response = await client.PostAsJsonAsync("/api/bookings", request, Ct);

        await ProblemAssert.IsProblemAsync(response, HttpStatusCode.BadRequest, code);
    }

    [Fact]
    public async Task Create_Returns400StartsInPast_ForAPastSlot()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var yesterday = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1);

        var response = await client.PostAsJsonAsync("/api/bookings", Request(1, Alfa, yesterday, 9, 10), Ct);

        await ProblemAssert.IsProblemAsync(response, HttpStatusCode.BadRequest, "STARTS_IN_PAST");
    }

    [Fact]
    public async Task Create_Returns409RoomInactive_ForADeactivatedRoom()
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/api/bookings", Request(1, Epsilon, UniqueDay(), 9, 10), Ct);

        await ProblemAssert.IsProblemAsync(response, HttpStatusCode.Conflict, "ROOM_INACTIVE");
    }

    [Fact]
    public async Task Create_Returns400_WhenTheRoomDoesNotExist()
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/api/bookings", Request(1, 999_999, UniqueDay(), 9, 10), Ct);

        await ProblemAssert.IsProblemAsync(response, HttpStatusCode.BadRequest, "VALIDATION_FAILED");
    }

    [Fact]
    public async Task Cancel_KeepsTheBookingVisibleAsCancelled_AndIsIdempotent()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var customer = await CreateCustomerAsync(client);
        var booking = await CreateBookingAsync(client, Request(customer.Id, Alfa, UniqueDay(), 9, 10));

        var first = await client.PostAsync($"/api/bookings/{booking.Id}/cancel", null, Ct);
        var second = await client.PostAsync($"/api/bookings/{booking.Id}/cancel", null, Ct);

        var cancelled = await first.Content.ReadFromJsonAsync<BookingResponse>(Ct);
        Assert.Equal(BookingStatus.Cancelled, cancelled?.Status);
        Assert.NotNull(cancelled?.CancelledAt);
        Assert.Equal(cancelled, await second.Content.ReadFromJsonAsync<BookingResponse>(Ct));
        Assert.Equal(cancelled, await client.GetFromJsonAsync<BookingResponse>($"/api/bookings/{booking.Id}", Ct));
    }

    [Fact]
    public async Task Cancel_Returns409BookingStarted_ForABookingInThePast()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        // Il seed dimostrativo ha prenotazioni di ieri.
        var now = DateTimeOffset.UtcNow;
        var past = (await client.GetFromJsonAsync<List<BookingResponse>>(Range(now.AddDays(-3), now), Ct))!
            .First(b => b.Status == BookingStatus.Confirmed && b.StartsAt < now);

        var response = await client.PostAsync($"/api/bookings/{past.Id}/cancel", null, Ct);

        await ProblemAssert.IsProblemAsync(response, HttpStatusCode.Conflict, "BOOKING_STARTED");
    }

    [Fact]
    public async Task Cancel_Returns404_WhenTheBookingDoesNotExist()
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        var response = await client.PostAsync("/api/bookings/999999/cancel", null, Ct);

        await ProblemAssert.IsProblemAsync(response, HttpStatusCode.NotFound, "NOT_FOUND");
    }

    [Fact]
    public async Task List_ReturnsBookingsIntersectingTheRange_CancelledIncluded_BordersExcluded()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var customer = await CreateCustomerAsync(client);
        var day = UniqueDay();
        var before = await CreateBookingAsync(client, Request(customer.Id, Alfa, day, 9, 10));
        var inside = await CreateBookingAsync(client, Request(customer.Id, Alfa, day, 10, 11));
        var cancelled = await CreateBookingAsync(client, Request(customer.Id, Beta, day, 11, 12));
        await client.PostAsync($"/api/bookings/{cancelled.Id}/cancel", null, Ct);

        var list = await client.GetFromJsonAsync<List<BookingResponse>>(Range(At(day, 10), At(day, 12)), Ct);

        Assert.NotNull(list);
        Assert.DoesNotContain(list, b => b.Id == before.Id);
        Assert.Contains(list, b => b.Id == inside.Id);
        Assert.Contains(list, b => b.Id == cancelled.Id && b.Status == BookingStatus.Cancelled);
    }

    [Fact]
    public async Task List_Returns400_WhenTheRangeIsLongerThan31Days()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var from = At(UniqueDay(), 0);

        var response = await client.GetAsync(Range(from, from.AddDays(32)), Ct);

        await ProblemAssert.IsProblemAsync(response, HttpStatusCode.BadRequest, "INVALID_TIME_RANGE");
    }

    [Fact]
    public async Task List_Returns400_WhenTheRangeIsMissing()
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        var response = await client.GetAsync("/api/bookings", Ct);

        await ProblemAssert.IsProblemAsync(response, HttpStatusCode.BadRequest, "VALIDATION_FAILED");
    }

    [Fact]
    public async Task ListForCustomer_ReturnsOnlyThatCustomersBookings()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var (mine, other) = (await CreateCustomerAsync(client), await CreateCustomerAsync(client));
        var day = UniqueDay();
        var booking = await CreateBookingAsync(client, Request(mine.Id, Alfa, day, 9, 10));
        await CreateBookingAsync(client, Request(other.Id, Beta, day, 9, 10));

        var list = await client.GetFromJsonAsync<List<BookingResponse>>($"/api/customers/{mine.Id}/bookings", Ct);

        Assert.Equal([booking.Id], list!.Select(b => b.Id));
    }

    [Fact]
    public async Task ListForCustomer_Returns404_WhenTheCustomerDoesNotExist()
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        var response = await client.GetAsync("/api/customers/999999/bookings", Ct);

        await ProblemAssert.IsProblemAsync(response, HttpStatusCode.NotFound, "NOT_FOUND");
    }

    [Fact]
    public async Task DeleteCustomer_Returns409HasBookings_EvenWhenTheBookingIsCancelled()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var customer = await CreateCustomerAsync(client);
        var booking = await CreateBookingAsync(client, Request(customer.Id, Alfa, UniqueDay(), 9, 10));
        await client.PostAsync($"/api/bookings/{booking.Id}/cancel", null, Ct);

        var response = await client.DeleteAsync($"/api/customers/{customer.Id}", Ct);

        await ProblemAssert.IsProblemAsync(response, HttpStatusCode.Conflict, "HAS_BOOKINGS");
    }

    [Fact]
    public async Task DeleteRoom_Returns409HasBookings()
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        // La sala Alfa ha prenotazioni dimostrative fin dal primo avvio.
        var response = await client.DeleteAsync($"/api/rooms/{Alfa}", Ct);

        await ProblemAssert.IsProblemAsync(response, HttpStatusCode.Conflict, "HAS_BOOKINGS");
    }

    /// <summary>
    /// Un giorno futuro diverso per ogni test: le classi condividono il database, e così le
    /// prenotazioni di un test non si scontrano con quelle di un altro né con il seed.
    /// </summary>
    private static DateOnly UniqueDay() =>
        DateOnly.FromDateTime(DateTime.UtcNow).AddDays(30 + Interlocked.Increment(ref nextDay));

    private static DateTimeOffset At(DateOnly day, int hour, int minute = 0) =>
        new(TimeZoneInfo.ConvertTimeToUtc(day.ToDateTime(new TimeOnly(hour, minute)), Rome), TimeSpan.Zero);

    private static CreateBookingRequest Request(int customerId, int roomId, DateOnly day, int startHour, int endHour) =>
        new(customerId, roomId, At(day, startHour), At(day, endHour), 2);

    private static string Range(DateTimeOffset from, DateTimeOffset to) =>
        $"/api/bookings?from={Uri.EscapeDataString(from.ToString("O"))}&to={Uri.EscapeDataString(to.ToString("O"))}";

    private static async Task<CustomerResponse> CreateCustomerAsync(HttpClient client)
    {
        var email = $"{Guid.NewGuid():N}@example.com";
        var response = await client.PostAsJsonAsync("/api/customers", new CreateCustomerRequest("Anna", "Prova", email), Ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<CustomerResponse>(Ct))!;
    }

    private static async Task<BookingResponse> CreateBookingAsync(HttpClient client, CreateBookingRequest request)
    {
        var response = await client.PostAsJsonAsync("/api/bookings", request, Ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<BookingResponse>(Ct))!;
    }
}
