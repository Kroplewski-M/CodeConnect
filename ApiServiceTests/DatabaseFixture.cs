using InfrastructureLayer;
using Microsoft.EntityFrameworkCore;

namespace ApiServiceTests;

public class DatabaseFixture : IDisposable
{
    public ApplicationDbContext Context { get; private set; }

    public DatabaseFixture()
    {
        var option = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        Context = new ApplicationDbContext(option);
    }
    public void Dispose()
    {
        Context?.Dispose();
    }
}

[CollectionDefinition("DatabaseCollection")]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture>{}