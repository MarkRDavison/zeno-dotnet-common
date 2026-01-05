namespace mark.davison.common.persistence.Helpers;

public interface IDataSeeder
{
    Task SeedDataAsync(DbContext dbContext, CancellationToken token);
}
