namespace mark.davison.common.persistence.tests.Repository;

[TestClass]
public class RepositoryTests
{
    private readonly IRepository _repository;

    private readonly IServiceProvider _serviceProvider;

    public RepositoryTests()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.AddDbContextFactory<TestDbContext>(_ => _
            .UseSqlite($"Data Source={Guid.NewGuid()}.db"));
        serviceCollection.AddTransient<IRepository>(_ =>
            new TestRepository(
                _.GetRequiredService<IDbContextFactory<TestDbContext>>(),
                _.GetRequiredService<ILogger<TestRepository>>())
            );
        _serviceProvider = serviceCollection.BuildServiceProvider();

        _repository = _serviceProvider.GetRequiredService<IRepository>();
    }

    [TestMethod]
    public async Task GetEntitiesWorks()
    {
        var authors = await _repository.GetEntitiesAsync<Author>(CancellationToken.None);

        Assert.IsFalse(authors.Any());
    }

    [TestMethod]
    public async Task GetEntityByIdWorks()
    {
        var author = await _repository.GetEntityAsync<Author>(Guid.NewGuid());

        Assert.IsNull(author);
    }

    [TestMethod]
    public async Task GetEntityByPredicateWorks()
    {
        var author = await _repository.GetEntityAsync<Author>(_ => _.Id == Guid.Empty);

        Assert.IsNull(author);
    }

    [TestMethod]
    public async Task GetEntitiesByPredicateWorks()
    {
        var authors = await _repository.GetEntitiesAsync<Author>(_ => _.Id == Guid.Empty);

        Assert.IsFalse(authors.Any());
    }

    [TestMethod]
    public async Task UpsetEntityRelationshipWOrks()
    {
        var author = new Author
        {
            Id = Guid.NewGuid()
        };
        var blog = new Blog
        {
            Id = Guid.NewGuid(),
            AuthorId = author.Id,
            Author = author
        };
        var post = new Post
        {
            Id = Guid.NewGuid(),
            BlogId = blog.Id,
            Blog = blog
        };

        await _repository.UpsertEntityAsync(post);

        Assert.IsNotNull(await _repository.GetEntityAsync<Post>(post.Id));
        Assert.IsNotNull(await _repository.GetEntityAsync<Blog>(blog.Id));
        Assert.IsNotNull(await _repository.GetEntityAsync<Author>(author.Id));
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public async Task UpsetDuplicateEntities_Throws()
    {
        var author = new Author
        {
            Id = Guid.NewGuid()
        };

        await _repository.UpsertEntitiesAsync(new List<Author> { author, author });
    }

    [TestMethod]
    public async Task UpsetNoDuplicateEntities_DoesNotThrow()
    {
        var author1 = new Author
        {
            Id = Guid.NewGuid()
        };
        var author2 = new Author
        {
            Id = Guid.NewGuid()
        };

        await _repository.UpsertEntitiesAsync(new List<Author> { author1, author2 });
    }

    [TestMethod]
    public async Task UpsetDuplicateEntitiesWithEmptyId_DoesNotThrow()
    {
        var author1 = new Author
        {
            Id = Guid.Empty
        };
        var author2 = new Author
        {
            Id = Guid.Empty
        };

        await _repository.UpsertEntitiesAsync(new List<Author> { author1, author2 });
    }

    [TestMethod]
    public async Task UpsetExistingEntities_Works()
    {
        var author = new Author
        {
            Id = Guid.Empty,
            FirstName = "Joe",
            LastName = "Blogs"
        };

        await _repository.UpsertEntitiesAsync(new List<Author> { author });

        string updatedName = "UPDATED";
        author.FirstName = updatedName;

        var updated = await _repository.UpsertEntitiesAsync(new List<Author> { author });

        Assert.AreEqual(1, updated.Count);
        Assert.AreEqual(author.Id, updated[0].Id);
        Assert.AreEqual(updatedName, updated[0].FirstName);
    }

    [TestMethod]
    public async Task UpsetExistingEntity_Works()
    {
        var author = new Author
        {
            Id = Guid.Empty,
            FirstName = "Joe",
            LastName = "Blogs"
        };

        await _repository.UpsertEntityAsync(author);

        string updatedName = "UPDATED";
        author.FirstName = updatedName;

        var updated = await _repository.UpsertEntityAsync(author);

        Assert.IsNotNull(updated);
        Assert.AreEqual(author.Id, updated.Id);
        Assert.AreEqual(updatedName, updated.FirstName);
    }

    [TestMethod]
    public async Task DeleteEntityWorks()
    {
        var author1 = new Author
        {
            Id = Guid.NewGuid()
        };
        var author2 = new Author
        {
            Id = Guid.NewGuid()
        };

        await _repository.UpsertEntitiesAsync(new List<Author> { author1, author2 });

        Assert.IsNotNull(await _repository.DeleteEntityAsync(author1));
    }

    [TestMethod]
    public async Task DeleteNonExistantEntityWorks()
    {
        var author1 = new Author
        {
            Id = Guid.NewGuid()
        };
        var author2 = new Author
        {
            Id = Guid.NewGuid()
        };

        await _repository.UpsertEntitiesAsync(new List<Author> { author1 });

        Assert.IsNull(await _repository.DeleteEntityAsync(author2));
    }

    [TestMethod]
    public async Task DeleteEntitiesWorks()
    {
        var author1 = new Author
        {
            Id = Guid.NewGuid()
        };
        var author2 = new Author
        {
            Id = Guid.NewGuid()
        };

        await _repository.UpsertEntitiesAsync(new List<Author> { author1, author2 });
        var entities = await _repository.DeleteEntitiesAsync(new List<Author> { author1, author2 });

        Assert.AreEqual(2, entities.Count);
    }

    [TestMethod]
    public async Task GetEntity_WithInclude_Works()
    {
        var author = new Author
        {
            Id = Guid.NewGuid()
        };
        var blog = new Blog
        {
            Id = Guid.NewGuid(),
            AuthorId = author.Id,
            Author = author
        };
        var post = new Post
        {
            Id = Guid.NewGuid(),
            BlogId = blog.Id,
            Blog = blog
        };

        await _repository.UpsertEntityAsync(post);

        var includes = new Expression<Func<Post, object>>[] {
            _ => _.Blog!,
            _ => _.Blog!.Author!,
        };

        var query = new QueryParameters();
        query.Include("Blog.Author");

        var fetchedPost = await _repository.GetEntityAsync<Post>(
            post.Id,
            query["include"]);

        Assert.IsNotNull(fetchedPost);
        Assert.IsNotNull(fetchedPost.Blog);
        Assert.IsNotNull(fetchedPost.Blog.Author);

        fetchedPost = await _repository.GetEntityAsync<Post>(
            post.Id,
            includes);

        Assert.IsNotNull(fetchedPost);
        Assert.IsNotNull(fetchedPost.Blog);
        Assert.IsNotNull(fetchedPost.Blog.Author);
    }

    [TestMethod]
    public async Task GetEntities_WithInclude_Works()
    {
        var author = new Author
        {
            Id = Guid.NewGuid()
        };
        var blog = new Blog
        {
            Id = Guid.NewGuid(),
            AuthorId = author.Id,
            Author = author
        };
        var post = new Post
        {
            Id = Guid.NewGuid(),
            BlogId = blog.Id,
            Blog = blog
        };

        await _repository.UpsertEntityAsync(post);

        var includes = new Expression<Func<Post, object>>[] {
            _ => _.Blog!,
            _ => _.Blog!.Author!,
        };

        var query = new QueryParameters();
        query.Include("Blog");
        query.Include("Blog.Author");

        var fetchedPost = (await _repository.GetEntitiesAsync<Post>(query["include"])).First();

        Assert.IsNotNull(fetchedPost);
        Assert.IsNotNull(fetchedPost.Blog);
        Assert.IsNotNull(fetchedPost.Blog.Author);

        fetchedPost = (await _repository.GetEntitiesAsync<Post>((string)null!, CancellationToken.None)).First();

        Assert.IsNotNull(fetchedPost);
        Assert.IsNull(fetchedPost.Blog);

        fetchedPost = fetchedPost = (await _repository.GetEntitiesAsync<Post>(includes)).First();

        Assert.IsNotNull(fetchedPost);
        Assert.IsNotNull(fetchedPost.Blog);
        Assert.IsNotNull(fetchedPost.Blog.Author);
    }
}
