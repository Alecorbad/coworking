using Coworking.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coworking.Api.Infrastructure.Configurations;

/// <summary>La forma della tabella dei clienti: lunghezze, email unica e righe dimostrative.</summary>
public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    /// <summary>Lunghezza massima di nome e cognome; la usano anche le validazioni delle richieste.</summary>
    public const int NameMaxLength = 100;

    /// <summary>Lunghezza massima dell'email.</summary>
    public const int EmailMaxLength = 256;

    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.Property(c => c.Id).HasIdentityOptions(startValue: SeedData.FirstGeneratedId);
        builder.Property(c => c.FirstName).HasMaxLength(NameMaxLength);
        builder.Property(c => c.LastName).HasMaxLength(NameMaxLength);
        builder.Property(c => c.Email).HasMaxLength(EmailMaxLength);
        // Il service controlla prima e risponde con un errore chiaro; l'indice ferma anche due richieste
        // con la stessa email arrivate nello stesso istante.
        builder.HasIndex(c => c.Email).IsUnique();

        builder.HasData(SeedData.Customers);
    }
}
