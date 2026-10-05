// Punto d'ingresso dell'API. La prima metà registra i servizi, la seconda costruisce il percorso
// che ogni richiesta attraversa: lì l'ordine conta, ed è numerato.

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

// La stringa di connessione si legge quando nasce il DbContext, non all'avvio: durante la build
// l'app parte senza configurazione, solo per scrivere il documento OpenAPI.
builder.Services.AddDbContext<AppDbContext>(options => options
    .UseNpgsql(builder.Configuration.GetConnectionString("Default")
        ?? throw new InvalidOperationException("Missing connection string 'ConnectionStrings:Default'."))
    .UseSnakeCaseNamingConvention());

// Fuso del coworking (Europe/Rome). Le regole sugli orari di apertura convertono gli istanti UTC in
// quest'ora locale prima di confrontarli.
builder.Services.AddSingleton(TimeZoneInfo.FindSystemTimeZoneById(builder.Configuration["TimeZone"]
    ?? throw new InvalidOperationException("Missing configuration key 'TimeZone'.")));

// Identity: utenti e hash delle password nel database; il cookie di sessione lo gestisce il
// middleware di autenticazione. Login, logout e "chi sono" stanno in AuthController; non esiste
// una registrazione pubblica.
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
        // Di base Identity risponde con un redirect a /Account/Login, che in un'API non ha senso:
        // meglio un 401 semplice, che il frontend trasforma nella pagina di accesso.
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
// Durante la build l'app parte senza database né password, solo per scrivere il documento OpenAPI:
// in quel caso i servizi d'avvio restano spenti.
if (System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name != "GetDocument.Insider")
{
    builder.Services.AddHostedService<ReceptionistSeeder>();
    // Va registrato dopo ReceptionistSeeder: i servizi partono in ordine, e il primo applica le migration.
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

// Le risposte sono solo JSON, così il documento OpenAPI non elenca anche text/plain e text/json.
// Ogni endpoint richiede la sessione, tranne quelli marcati [AllowAnonymous]: solo il login.
builder.Services.AddControllers(options =>
{
    options.Filters.Add(new ProducesAttribute("application/json"));
    options.Filters.Add(new AuthorizeFilter());
}).AddJsonOptions(options => ConfigureJson(options.JsonSerializerOptions));
// Le stesse regole JSON valgono per il documento OpenAPI, che legge queste opzioni e non quelle di MVC.
builder.Services.ConfigureHttpJsonOptions(options => ConfigureJson(options.SerializerOptions));
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks().AddDbContextCheck<AppDbContext>();

var app = builder.Build();

// Cammino di una richiesta; l'ordine conta.
// 1. UseExceptionHandler: intercetta le eccezioni delle fasi successive, le scrive nel log e risponde
//    500 con un ProblemDetails (code INTERNAL_ERROR), senza dettagli interni.
app.UseExceptionHandler();
// 2. UseStatusCodePages: dà un corpo ProblemDetails alle risposte 401 e 404 nate senza corpo.
app.UseStatusCodePages();

// 3. UseAuthentication: legge il cookie di sessione e stabilisce chi è l'utente.
app.UseAuthentication();
// 4. UseAuthorization: applica le regole di accesso dichiarate sugli endpoint.
app.UseAuthorization();

// 5. Endpoint. OpenAPI e Scalar restano attivi in ogni ambiente: chi valuta il progetto li apre dal container.
app.MapOpenApi();
app.MapScalarApiReference();
app.MapHealthChecks("/health");
// 6. I controller, tutti protetti dal filtro globale tranne il login.
app.MapControllers();

app.Run();

// JSON rigoroso. I numeri devono arrivare come numeri, non come testo ("4"): così il contratto
// dichiara integer e nel frontend restano number. Un campo obbligatorio assente produce un 400,
// invece di diventare in silenzio un valore predefinito come 0, 00:00 o false.
static void ConfigureJson(JsonSerializerOptions json)
{
    json.NumberHandling = JsonNumberHandling.Strict;
    json.RespectRequiredConstructorParameters = true;
}
