// Ruolo:   forma della tabella customers
// Riceve:  il modello Customer
// Produce: colonne, indice unico sull'email, token di concorrenza, righe del seed
// Regola:  ogni modifica qui richiede una migration
// Vedi:    Domain/Customer.cs, Infrastructure/SeedData.cs

using Coworking.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coworking.Api.Infrastructure.Configurations;

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public const int NameMaxLength = 100;
    public const int EmailMaxLength = 256;

    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.Property(c => c.Id).HasIdentityOptions(startValue: SeedData.FirstGeneratedId);
        builder.Property(c => c.FirstName).HasMaxLength(NameMaxLength);
        builder.Property(c => c.LastName).HasMaxLength(NameMaxLength);
        builder.Property(c => c.Email).HasMaxLength(EmailMaxLength);
        // Il service controlla prima e risponde con un errore chiaro; l'indice ferma anche due
        // richieste con la stessa email arrivate nello stesso istante.
        builder.HasIndex(c => c.Email).IsUnique();
        builder.Property(c => c.Version).IsRowVersion();

        builder.HasData(SeedData.Customers);
    }
}
