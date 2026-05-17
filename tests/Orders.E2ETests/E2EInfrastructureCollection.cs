namespace Orders.E2ETests;

[CollectionDefinition(Name)]
public sealed class E2EInfrastructureCollection : ICollectionFixture<E2EInfrastructureFixture>
{
    public const string Name = "E2E Infrastructure";
}
