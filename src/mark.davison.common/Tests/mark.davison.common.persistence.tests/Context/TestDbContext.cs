using mark.davison.common.server.abstractions.Identification;

namespace mark.davison.common.persistence.tests.Context;

public class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions options) : base(options)
    {
        Database.EnsureCreated();
    }

    public DbSet<Author> Authors => Set<Author>();
    public DbSet<Blog> Blogs => Set<Blog>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<User> Users => Set<User>();
}
