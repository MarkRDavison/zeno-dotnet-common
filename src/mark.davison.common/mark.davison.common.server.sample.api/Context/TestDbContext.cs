namespace mark.davison.common.server.sample.api.Context;

[DatabaseMigrationAssembly(DatabaseType.Sqlite)]
public class TestDbContext : DbContextBase<TestDbContext>
{
    public TestDbContext(DbContextOptions options) : base(options)
    {
        Database.EnsureCreated();
    }

    public DbSet<Author> Authors => Set<Author>();
    public DbSet<Blog> Blogs => Set<Blog>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Comment> Comments => Set<Comment>();
}
