using Microsoft.EntityFrameworkCore;

namespace Coworking.Api.Infrastructure;

/// <summary>
/// Accesso al database: i service lo usano direttamente, senza repository (D-3).
/// Le entità si aggiungono con le feature.
/// </summary>
public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
}
