using System.Net;
using System.Net.Http.Json;
using Coworking.Api.Features.Customers;

namespace Coworking.Api.IntegrationTests.Features;

[Collection(SharedApi.Name)]
public sealed class CustomersTests(ApiFactory factory)
{
    private static readonly CancellationToken Ct = TestContext.Current.CancellationToken;

    [Fact]
    public async Task List_ReturnsSeededCustomers_OrderedByLastName()
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        var customers = await client.GetFromJsonAsync<List<CustomerResponse>>("/api/customers", Ct);

        Assert.NotNull(customers);
        Assert.Contains(customers, c => c.Email == "giulia.bianchi@example.com");
        Assert.Equal(customers.OrderBy(c => c.LastName).ThenBy(c => c.FirstName), customers);
    }

    [Fact]
    public async Task Create_Returns201_WithNormalizedEmailAndServerRegistrationTime()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var email = UniqueEmail();
        var before = DateTimeOffset.UtcNow.AddSeconds(-5);

        var response = await client.PostAsJsonAsync(
            "/api/customers", new CreateCustomerRequest(" Anna ", " Verdi ", $"  {email.ToUpperInvariant()} "), Ct);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<CustomerResponse>(Ct);
        Assert.NotNull(created);
        Assert.Equal(("Anna", "Verdi", email), (created.FirstName, created.LastName, created.Email));
        Assert.InRange(created.RegisteredAt, before, DateTimeOffset.UtcNow.AddSeconds(5));
        var read = await client.GetFromJsonAsync<CustomerResponse>(response.Headers.Location, Ct);
        Assert.Equal(created, read);
    }

    [Fact]
    public async Task Create_Returns409_WhenEmailDiffersOnlyByCase()
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync(
            "/api/customers", new CreateCustomerRequest("Giulia", "Bianchi", "Giulia.Bianchi@Example.com"), Ct);

        await ProblemAssert.IsProblemAsync(response, HttpStatusCode.Conflict, "EMAIL_TAKEN");
    }

    [Fact]
    public async Task Create_Returns400WithFieldErrors_WhenBodyIsNotValid()
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/api/customers", new CreateCustomerRequest("", "Verdi", "not-an-email"), Ct);

        var body = await ProblemAssert.IsProblemAsync(response, HttpStatusCode.BadRequest, "VALIDATION_FAILED");
        var errors = body.GetProperty("errors");
        Assert.True(errors.TryGetProperty("FirstName", out _));
        Assert.True(errors.TryGetProperty("Email", out _));
    }

    [Fact]
    public async Task Update_Returns409_WhenAnotherOperatorSavedFirst()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var customer = await CreateAsync(client);

        // Due operatori partono dalla stessa version: il primo salva, il secondo arriva tardi.
        var first = await client.PutAsJsonAsync($"/api/customers/{customer.Id}", ToUpdate(customer) with { LastName = "Primo" }, Ct);
        var second = await client.PutAsJsonAsync($"/api/customers/{customer.Id}", ToUpdate(customer) with { LastName = "Secondo" }, Ct);

        first.EnsureSuccessStatusCode();
        await ProblemAssert.IsProblemAsync(second, HttpStatusCode.Conflict, "CONCURRENT_UPDATE");
        var stored = await client.GetFromJsonAsync<CustomerResponse>($"/api/customers/{customer.Id}", Ct);
        Assert.Equal("Primo", stored!.LastName);
    }

    [Fact]
    public async Task Update_Returns409_WhenEmailBelongsToAnotherCustomer()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var customer = await CreateAsync(client);

        var response = await client.PutAsJsonAsync(
            $"/api/customers/{customer.Id}", ToUpdate(customer) with { Email = "marco.rossi@example.com" }, Ct);

        await ProblemAssert.IsProblemAsync(response, HttpStatusCode.Conflict, "EMAIL_TAKEN");
    }

    [Fact]
    public async Task Update_KeepsRegistrationTime()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var customer = await CreateAsync(client);

        var response = await client.PutAsJsonAsync($"/api/customers/{customer.Id}", ToUpdate(customer) with { FirstName = "Nuovo" }, Ct);

        response.EnsureSuccessStatusCode();
        var updated = await response.Content.ReadFromJsonAsync<CustomerResponse>(Ct);
        Assert.Equal(customer.RegisteredAt, updated!.RegisteredAt);
        Assert.NotEqual(customer.Version, updated.Version);
    }

    [Fact]
    public async Task Delete_Returns204_ThenTheCustomerIsGone()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var customer = await CreateAsync(client);

        var delete = await client.DeleteAsync($"/api/customers/{customer.Id}", Ct);
        var read = await client.GetAsync($"/api/customers/{customer.Id}", Ct);

        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
        await ProblemAssert.IsProblemAsync(read, HttpStatusCode.NotFound, "NOT_FOUND");
    }

    [Theory]
    [InlineData("GET")]
    [InlineData("PUT")]
    [InlineData("DELETE")]
    public async Task UnknownCustomer_Returns404(string method)
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var request = new HttpRequestMessage(new HttpMethod(method), "/api/customers/999999");
        if (method == "PUT")
        {
            request.Content = JsonContent.Create(new UpdateCustomerRequest("Anna", "Verdi", UniqueEmail(), 0));
        }

        var response = await client.SendAsync(request, Ct);

        await ProblemAssert.IsProblemAsync(response, HttpStatusCode.NotFound, "NOT_FOUND");
    }

    private static string UniqueEmail() => $"{Guid.NewGuid():N}@example.com";

    private static UpdateCustomerRequest ToUpdate(CustomerResponse customer) =>
        new(customer.FirstName, customer.LastName, customer.Email, customer.Version);

    private static async Task<CustomerResponse> CreateAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/customers", new CreateCustomerRequest("Anna", "Verdi", UniqueEmail()), Ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<CustomerResponse>(Ct))!;
    }
}
