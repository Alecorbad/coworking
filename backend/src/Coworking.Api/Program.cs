// Ruolo:   compone l'applicazione: servizi e pipeline HTTP
// Riceve:  configurazione (appsettings e variabili d'ambiente)
// Produce: l'API in ascolto
// Regola:  l'ordine dei middleware conta: vedi la sequenza numerata in fondo
// Vedi:    Common/, Features/Auth/, Infrastructure/

using Coworking.Api.Common;
using Coworking.Api.Features.Auth;
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

builder.Services.AddSingleton(CoworkingTimeZone.FromId(builder.Configuration["TimeZone"]
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
        // Il redirect predefinito (302 verso /Account/Login) non ha senso in un'API: 401 e 403
        // semplici, che il frontend traduce in pagina di login.
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    }));
builder.Services.AddAuthorization();
builder.Services.AddScoped<AuthService>();
// La build avvia l'app (host compreso) per scrivere il documento OpenAPI, senza database né
// password: in quel caso il seed non parte.
if (System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name != "GetDocument.Insider")
{
    builder.Services.AddHostedService<ReceptionistSeeder>();
}

builder.Services.AddProblemDetails(options => options.CustomizeProblemDetails = context =>
{
    var problem = context.ProblemDetails;
    if (!problem.Extensions.ContainsKey("code") && ProblemDetailsExtensions.DefaultCode(problem.Status) is { } code)
    {
        problem.Extensions["code"] = code;
    }
});
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Solo JSON in uscita: il documento OpenAPI non elenca text/plain né text/json.
// Ogni controller richiede la sessione tranne dove c'è [AllowAnonymous] (il solo login).
builder.Services.AddControllers(options =>
{
    options.Filters.Add(new ProducesAttribute("application/json"));
    options.Filters.Add(new AuthorizeFilter());
    options.Filters.Add<ProblemContentTypeFilter>();
});
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks().AddDbContextCheck<AppDbContext>();

var app = builder.Build();

// Cammino di una richiesta; l'ordine conta.
// 1. UseExceptionHandler: cattura le eccezioni delle fasi successive e risponde 500.
app.UseExceptionHandler();
// 2. UseStatusCodePages: dà un corpo ProblemDetails alle risposte 401, 403 e 404 nate senza corpo.
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
