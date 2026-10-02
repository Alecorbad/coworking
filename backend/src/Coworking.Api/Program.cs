using Coworking.Api.Common;
using Coworking.Api.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Stringa di connessione letta alla creazione del DbContext, non all'avvio: la build
// avvia l'app senza configurazione per scrivere il documento OpenAPI (D-14).
builder.Services.AddDbContext<AppDbContext>(options => options
    .UseNpgsql(builder.Configuration.GetConnectionString("Default")
        ?? throw new InvalidOperationException("Missing connection string 'ConnectionStrings:Default'."))
    .UseSnakeCaseNamingConvention());

builder.Services.AddSingleton(CoworkingTimeZone.FromId(builder.Configuration["TimeZone"]
    ?? throw new InvalidOperationException("Missing configuration key 'TimeZone'.")));

// Solo JSON in uscita: il documento OpenAPI non elenca text/plain né text/json.
builder.Services.AddControllers(options => options.Filters.Add(new ProducesAttribute("application/json")));
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks().AddDbContextCheck<AppDbContext>();

var app = builder.Build();

// OpenAPI e Scalar esposti in ogni ambiente: chi valuta il progetto li usa dal container (D-19).
app.MapOpenApi();
app.MapScalarApiReference();

app.MapHealthChecks("/health");
app.MapControllers();

app.Run();
