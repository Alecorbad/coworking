using System.Net;
using System.Net.Http.Json;
using Coworking.Api.Features.Rooms;

namespace Coworking.Api.IntegrationTests.Features;

[Collection(SharedApi.Name)]
public sealed class RoomsTests(ApiFactory factory)
{
    private static readonly CancellationToken Ct = TestContext.Current.CancellationToken;

    [Fact]
    public async Task List_ReturnsSeededRooms_IncludingInactiveOnes()
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        var rooms = await client.GetFromJsonAsync<List<RoomResponse>>("/api/rooms", Ct);

        Assert.NotNull(rooms);
        Assert.Contains(rooms, r => r is { Name: "Sala Alfa", IsActive: true });
        Assert.Contains(rooms, r => r is { Name: "Sala Epsilon", IsActive: false });
    }

    [Fact]
    public async Task Create_Returns201WithLocation_AndTrimsTheName()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var name = UniqueName();

        var response = await client.PostAsJsonAsync("/api/rooms", Valid($"  {name}  "), Ct);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<RoomResponse>(Ct);
        Assert.NotNull(created);
        Assert.Equal(name, created.Name);
        Assert.True(created.IsActive);
        var read = await client.GetFromJsonAsync<RoomResponse>(response.Headers.Location, Ct);
        Assert.Equal(created, read);
    }

    [Fact]
    public async Task Create_Returns409_WhenNameIsTaken()
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/api/rooms", Valid("Sala Alfa"), Ct);

        await ProblemAssert.IsProblemAsync(response, HttpStatusCode.Conflict, "ROOM_NAME_TAKEN");
    }

    [Theory]
    [InlineData(18, 0, 9, 0)]
    [InlineData(9, 0, 9, 0)]
    [InlineData(9, 15, 18, 0)]
    public async Task Create_Returns400_WhenOpeningHoursAreNotValid(int openH, int openM, int closeH, int closeM)
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var request = Valid(UniqueName()) with { OpeningTime = new(openH, openM), ClosingTime = new(closeH, closeM) };

        var response = await client.PostAsJsonAsync("/api/rooms", request, Ct);

        await ProblemAssert.IsProblemAsync(response, HttpStatusCode.BadRequest, "INVALID_OPENING_HOURS");
    }

    [Fact]
    public async Task Create_Returns400WithFieldErrors_WhenBodyIsNotValid()
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/api/rooms", Valid("") with { Capacity = 0 }, Ct);

        var body = await ProblemAssert.IsProblemAsync(response, HttpStatusCode.BadRequest, "VALIDATION_FAILED");
        var errors = body.GetProperty("errors");
        Assert.True(errors.TryGetProperty("Name", out _));
        Assert.True(errors.TryGetProperty("Capacity", out _));
    }

    [Fact]
    public async Task Create_Returns400_WhenARequiredFieldIsMissing()
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        // Senza openingTime non si deve salvare una sala che apre a mezzanotte.
        var response = await client.PostAsJsonAsync(
            "/api/rooms", new { name = UniqueName(), capacity = 4, closingTime = "18:00:00" }, Ct);

        await ProblemAssert.IsProblemAsync(response, HttpStatusCode.BadRequest, "VALIDATION_FAILED");
    }

    [Fact]
    public async Task Update_DeactivatesRoom()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var room = await CreateAsync(client);

        var response = await client.PutAsJsonAsync($"/api/rooms/{room.Id}", ToUpdate(room) with { IsActive = false }, Ct);

        response.EnsureSuccessStatusCode();
        var updated = await response.Content.ReadFromJsonAsync<RoomResponse>(Ct);
        Assert.NotNull(updated);
        Assert.False(updated.IsActive);
    }

    [Fact]
    public async Task Update_Returns409_WhenRenamingToAnotherRoomsName()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var room = await CreateAsync(client);

        var response = await client.PutAsJsonAsync($"/api/rooms/{room.Id}", ToUpdate(room) with { Name = "Sala Beta" }, Ct);

        await ProblemAssert.IsProblemAsync(response, HttpStatusCode.Conflict, "ROOM_NAME_TAKEN");
    }

    [Fact]
    public async Task Update_Succeeds_WhenTheRoomKeepsItsOwnName()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var room = await CreateAsync(client);

        var response = await client.PutAsJsonAsync(
            $"/api/rooms/{room.Id}", ToUpdate(room) with { Capacity = 8 }, Ct);

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Delete_Returns204_ThenTheRoomIsGone()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var room = await CreateAsync(client);

        var delete = await client.DeleteAsync($"/api/rooms/{room.Id}", Ct);
        var read = await client.GetAsync($"/api/rooms/{room.Id}", Ct);

        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
        await ProblemAssert.IsProblemAsync(read, HttpStatusCode.NotFound, "NOT_FOUND");
    }

    [Theory]
    [InlineData("GET")]
    [InlineData("PUT")]
    [InlineData("DELETE")]
    public async Task UnknownRoom_Returns404(string method)
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var request = new HttpRequestMessage(new HttpMethod(method), "/api/rooms/999999");
        if (method == "PUT")
        {
            request.Content = JsonContent.Create(new UpdateRoomRequest(UniqueName(), 4, new(9, 0), new(18, 0), true));
        }

        var response = await client.SendAsync(request, Ct);

        await ProblemAssert.IsProblemAsync(response, HttpStatusCode.NotFound, "NOT_FOUND");
    }

    private static string UniqueName() => $"Sala {Guid.NewGuid():N}"[..20];

    private static CreateRoomRequest Valid(string name) => new(name, 6, new(9, 0), new(18, 30));

    private static UpdateRoomRequest ToUpdate(RoomResponse room) =>
        new(room.Name, room.Capacity, room.OpeningTime, room.ClosingTime, room.IsActive);

    private static async Task<RoomResponse> CreateAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/rooms", Valid(UniqueName()), Ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<RoomResponse>(Ct))!;
    }
}
