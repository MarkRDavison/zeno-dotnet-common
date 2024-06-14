namespace mark.davison.example.api.persistence;

public sealed class ExampleDbContext : DbContextBase<ExampleDbContext>, IExampleDbContext
{
    public ExampleDbContext(DbContextOptions options) : base(options)
    {
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserEntityConfiguration).Assembly);
    }

    public DbSet<User> Users => Set<User>();
}
