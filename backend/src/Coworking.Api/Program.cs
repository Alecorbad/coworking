// Ruolo:   compone l'applicazione: servizi e pipeline HTTP
// Riceve:  configurazione (appsettings e variabili d'ambiente)
// Produce: l'API in ascolto
// Regola:  l'ordine dei middleware conta: vedi la sequenza numerata in fondo
// Vedi:    Common/, Features/, Infrastructure/

using System.Text.Json;
using System.Text.Json.Serialization;
using Coworking.Api.Common;
using Coworking.Api.Features.Auth;
using Coworking.Api.Features.Bookings;
using Coworking.Api.Features.Customers;
using Coworking.Api.Features.Rooms;
using Coworking.Api.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Stringa di connessione letta alla creazione del DbContext, non all'avvio: la build
// avvia l'app senza configurazione per scrivere il documento OpenAPI.
builder.Services.AddDbContext<AppDbContext>(options => options
    .UseNpgsql(builder.Configuration.GetConnectionString("Default")
        ?? throw new InvalidOperationException("Missing connection string 'ConnectionStrings:Default'."))
    .UseSnakeCaseNamingConvention());

// Fuso del coworking: le regole sugli orari di apertura leggono gli istanti UTC in quest'ora locale.
builder.Services.AddSingleton(TimeZoneInfo.FindSystemTimeZoneById(builder.Configuration["TimeZone"]
    ?? throw new InvalidOperationException("Missing configuration key 'TimeZone'.")));

// Identity: utenti e hash delle password nel database; il cookie di sessione lo gestisce il
// middleware di autenticazione. Gli endpoint di accesso sono quelli di AuthController e non
// c'è registrazione pubblica.
builder.Services.AddIdentityCore<IdentityUser>().AddEntityFrameworkStores<AppDbContext>().AddSignInManager();
builder.Services
    .AddAuthentication(IdentityConstants.ApplicationScheme)
    .AddIdentityCookies(cookies => cookies.ApplicationCookie!.Configure(options =>
    {
        options.Cookie.Name = "coworking.session";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        // Il redirect predefinito (302 verso /Account/Login) non ha senso in un'API: un 401
        // semplice, che il frontend traduce in pagina di login. Senza ruoli un 403 non capita mai.
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
    }));
builder.Services.AddAuthorization();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<RoomsService>();
builder.Services.AddScoped<CustomersService>();
builder.Services.AddScoped<BookingsService>();
// La build avvia l'app (host compreso) per scrivere il documento OpenAPI, senza database né
// password: in quel caso il seed non parte.
if (System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name != "GetDocument.Insider")
{
    builder.Services.AddHostedService<ReceptionistSeeder>();
    // Dopo ReceptionistSeeder: i servizi partono in ordine di registrazione, e quello applica le migration.
    builder.Services.AddHostedService<DemoBookingsSeeder>();
}

builder.Services.AddProblemDetails(options => options.CustomizeProblemDetails = context =>
{
    var problem = context.ProblemDetails;
    if (!problem.Extensions.ContainsKey("code") && ProblemDetailsExtensions.DefaultCode(problem.Status) is { } code)
    {
        problem.Extensions["code"] = code;
    }
});

// Solo JSON in uscita: il documento OpenAPI non elenca text/plain né text/json.
// Ogni controller richiede la sessione tranne dove c'è [AllowAnonymous] (il solo login).
builder.Services.AddControllers(options =>
{
    options.Filters.Add(new ProducesAttribute("application/json"));
    options.Filters.Add(new AuthorizeFilter());
}).AddJsonOptions(options => ConfigureJson(options.JsonSerializerOptions));
// Le stesse regole per il documento OpenAPI, che legge le opzioni JSON minimali e non quelle di MVC.
builder.Services.ConfigureHttpJsonOptions(options => ConfigureJson(options.SerializerOptions));
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks().AddDbContextCheck<AppDbContext>();

var app = builder.Build();

// Cammino di una richiesta; l'ordine conta.
// 1. UseExceptionHandler: cattura le eccezioni delle fasi successive, le scrive nel log e risponde
//    500 ProblemDetails (code INTERNAL_ERROR, nessun dettaglio interno nella risposta).
app.UseExceptionHandler();
// 2. UseStatusCodePages: dà un corpo ProblemDetails alle risposte 401 e 404 nate senza corpo.
app.UseStatusCodePages();

// 3. UseAuthentication: legge il cookie di sessione e stabilisce chi è l'utente.
app.UseAuthentication();
// 4. UseAuthorization: applica le regole di accesso dichiarate sugli endpoint.
app.UseAuthorization();

// 5. Endpoint. OpenAPI e Scalar sono esposti in ogni ambiente: chi valuta il progetto li usa dal container.
app.MapOpenApi();
app.MapScalarApiReference();
app.MapHealthChecks("/health");
// 6. I controller: il filtro globale richiede la sessione, tranne dove c'è [AllowAnonymous] (il solo login).
app.MapControllers();

app.Run();

// Numeri solo come numeri (non "4" fra virgolette): il contratto dichiara integer e i tipi del
// frontend restano number. Un campo obbligatorio assente è un 400, non un valore predefinito
// silenzioso (0, 00:00, false).
static void ConfigureJson(JsonSerializerOptions json)
{
    json.NumberHandling = JsonNumberHandling.Strict;
    json.RespectRequiredConstructorParameters = true;
}
