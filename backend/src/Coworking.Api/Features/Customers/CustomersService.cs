// Ruolo:   regole dei clienti: elenco, creazione, modifica, cancellazione
// Riceve:  richieste già validate nella forma dal controller
// Produce: Result<CustomerResponse> (o di un elenco); errori attesi come ApiError
// Regola:  la modifica parte dalla version letta dal client: se la riga è cambiata, 409
// Vedi:    Features/Customers/CustomersController.cs, Infrastructure/Configurations/CustomerConfiguration.cs

using Coworking.Api.Common;
using Coworking.Api.Domain;
using Coworking.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Coworking.Api.Features.Customers;

public sealed class CustomersService(AppDbContext db, TimeProvider clock)
{
    private const string EmailIndex = "ix_customers_email";

    /// <summary>Tutti i clienti in ordine di cognome e nome.</summary>
    public async Task<IReadOnlyList<CustomerResponse>> ListAsync() =>
        await db.Customers.AsNoTracking()
            .OrderBy(c => c.LastName).ThenBy(c => c.FirstName)
            .Select(c => CustomerResponse.From(c))
            .ToListAsync();

    public async Task<Result<CustomerResponse>> GetAsync(int id) =>
        await db.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id) is { } customer
            ? CustomerResponse.From(customer)
            : NotFound(id);

    public async Task<Result<CustomerResponse>> CreateAsync(CreateCustomerRequest request)
    {
        var email = Customer.NormalizeEmail(request.Email);
        if (await EmailTakenAsync(email, exceptId: null))
        {
            return EmailTaken(email);
        }

        // PostgreSQL conserva i microsecondi, .NET i decimi di microsecondo: senza troncare, la
        // risposta della creazione e la rilettura successiva mostrerebbero due istanti diversi.
        var now = clock.GetUtcNow();
        var customer = new Customer
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = email,
            RegisteredAt = now.AddTicks(-(now.Ticks % TimeSpan.TicksPerMicrosecond)),
        };
        db.Customers.Add(customer);
        return await SaveAsync(customer);
    }

    public async Task<Result<CustomerResponse>> UpdateAsync(int id, UpdateCustomerRequest request)
    {
        var customer = await db.Customers.FirstOrDefaultAsync(c => c.Id == id);
        if (customer is null)
        {
            return NotFound(id);
        }

        var email = Customer.NormalizeEmail(request.Email);
        if (await EmailTakenAsync(email, exceptId: id))
        {
            return EmailTaken(email);
        }

        // La version del client diventa il valore «originale»: l'UPDATE ha WHERE xmin = version,
        // e se nel frattempo un altro operatore ha salvato non tocca nessuna riga.
        db.Entry(customer).Property(c => c.Version).OriginalValue = request.Version;
        customer.FirstName = request.FirstName.Trim();
        customer.LastName = request.LastName.Trim();
        customer.Email = email;
        return await SaveAsync(customer);
    }

    /// <summary>
    /// Cancella un cliente senza prenotazioni. Con prenotazioni la risposta sarà 409 HAS_BOOKINGS
    /// (le prenotazioni arrivano con la loro feature).
    /// </summary>
    public async Task<Result<bool>> DeleteAsync(int id)
    {
        var deleted = await db.Customers.Where(c => c.Id == id).ExecuteDeleteAsync();
        return deleted == 0 ? NotFound(id) : true;
    }

    private Task<bool> EmailTakenAsync(string email, int? exceptId) =>
        db.Customers.AnyAsync(c => c.Id != exceptId && c.Email == email);

    private async Task<Result<CustomerResponse>> SaveAsync(Customer customer)
    {
        try
        {
            await db.SaveChangesAsync();
            return CustomerResponse.From(customer);
        }
        catch (DbUpdateConcurrencyException)
        {
            return new ApiError(ErrorKind.Conflict, "CONCURRENT_UPDATE",
                $"Customer {customer.Id} was changed by someone else after it was read.");
        }
        catch (DbUpdateException exception) when (exception.IsUniqueViolation(EmailIndex))
        {
            return EmailTaken(customer.Email);
        }
    }

    private static ApiError NotFound(int id) => new(ErrorKind.NotFound, "NOT_FOUND", $"Customer {id} does not exist.");

    private static ApiError EmailTaken(string email) =>
        new(ErrorKind.Conflict, "EMAIL_TAKEN", $"A customer with email '{email}' already exists.");
}
