namespace Coworking.Api.IntegrationTests;

/// <summary>
/// Raggruppa le classi di test che usano la stessa <see cref="ApiFactory"/>: xUnit le esegue una dopo
/// l'altra, senza avviare l'API più volte.
/// </summary>
[CollectionDefinition(Name)]
public sealed class SharedApi : ICollectionFixture<ApiFactory>
{
    public const string Name = "api";
}
