namespace Coworking.Api.IntegrationTests;

[CollectionDefinition(Name)]
public sealed class SharedApi : ICollectionFixture<ApiFactory>
{
    public const string Name = "api";
}
