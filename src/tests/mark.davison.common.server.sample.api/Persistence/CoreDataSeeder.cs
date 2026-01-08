
namespace mark.davison.common.server.sample.api.Persistence;

public class CoreDataSeeder : IDataSeeder
{
    public async Task SeedDataAsync(DbContext dbContext, CancellationToken token)
    {
        await Task.CompletedTask;
    }

    public Task SeedUserDataAsync(Guid userId, DbContext dbContext, CancellationToken token)
    {
        return Task.CompletedTask;
    }
}
