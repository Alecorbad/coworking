using Coworking.Api.Common;
using Coworking.Api.Domain;
using Coworking.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Coworking.Api.Features.Customers;

/// <summary>
/// Le regole dei clienti. L'email si normalizza e deve essere unica (<c>EMAIL_TAKEN</c>); un cliente
/// con prenotazioni non si cancella (<c>HAS_BOOKINGS</c>).
/// </summary>
public sealed class CustomersService(AppDbContext db)
{
    private const string EmailIndex = "ix_customers_email";

    /// <summary>Tutti i clienti in ordine di cognome e nome.</summary>
    public async Task<IReadOnlyList<CustomerResponse>> ListAsync() =>
        await db.Customers.AsNoTracking()
            .OrderBy(c => c.LastName).ThenBy(c => c.FirstName)
            .Select(c => CustomerResponse.From(c))
            .ToListAsync();

    /// <summary>Un cliente, oppure <c>NOT_FOUND</c>.</summary>
    public async Task<Result<CustomerResponse>> GetAsync(int id) =>
        await db.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id) is { } customer
            ? CustomerResponse.From(customer)
            : Errors.NotFound("Customer", id);

    /// <summary>Registra un cliente con l'email normalizzata, se non è già usata.</summary>
    public async Task<Result<CustomerResponse>> CreateAsync(CreateCustomerRequest request)
    {
        var email = Customer.NormalizeEmail(request.Email);
        if (await EmailTakenAsync(email, exceptId: null))
        {
            return Errors.EmailTaken(email);
        }

        var customer = new Customer
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = email,
            RegisteredAt = DbClock.UtcNow(),
        };
        db.Customers.Add(customer);
        return await SaveAsync(customer);
    }

    /// <summary>Aggiorna un cliente. Vince l'ultimo salvataggio: non c'è controllo di versione.</summary>
    public async Task<Result<CustomerResponse>> UpdateAsync(int id, UpdateCustomerRequest request)
    {
        var customer = await db.Customers.FirstOrDefaultAsync(c => c.Id == id);
        if (customer is null)
        {
            return Errors.NotFound("Customer", id);
        }

        var email = Customer.NormalizeEmail(request.Email);
        if (await EmailTakenAsync(email, exceptId: id))
        {
            return Errors.EmailTaken(email);
        }

        customer.FirstName = request.FirstName.Trim();
        customer.LastName = request.LastName.Trim();
        customer.Email = email;
        return await SaveAsync(customer);
    }

    /// <summary>
    /// Cancella un cliente che non ha prenotazioni, nemmeno annullate, perché lo storico non perda il
    /// riferimento. Se ne ha, risponde <c>HAS_BOOKINGS</c>.
    /// </summary>
    public async Task<Result<bool>> DeleteAsync(int id)
    {
        if (await db.Bookings.AnyAsync(b => b.CustomerId == id))
        {
            return Errors.HasBookings("Customer", id);
        }

        var deleted = await db.Customers.Where(c => c.Id == id).ExecuteDeleteAsync();
        return deleted == 0 ? Errors.NotFound("Customer", id) : true;
    }

    private Task<bool> EmailTakenAsync(string email, int? exceptId) =>
        db.Customers.AnyAsync(c => c.Id != exceptId && c.Email == email);

    // Se un'altra richiesta ha preso la stessa email un attimo prima, la ferma l'indice unico.
    private async Task<Result<CustomerResponse>> SaveAsync(Customer customer)
    {
        try
        {
            await db.SaveChangesAsync();
            return CustomerResponse.From(customer);
        }
        catch (DbUpdateException exception) when (exception.IsViolationOf(EmailIndex))
        {
            return Errors.EmailTaken(customer.Email);
        }
    }
}
