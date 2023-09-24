namespace mark.davison.common.server.integrationtests.Tests.EventDriven;

[TestClass]
public class RepositoryBackedChangesetQueueTests : IntegrationTestBase<SampleApplicationFactory, AppSettings>
{
    private readonly List<Author> _authors = new()
    {
        new Author { Id = Guid.NewGuid(), FirstName = "a" },
        new Author { Id = Guid.NewGuid(), FirstName = "ab" },
        new Author { Id = Guid.NewGuid(), FirstName = "b" },
        new Author { Id = Guid.NewGuid(), FirstName = "ba" }
    };

    private readonly List<Blog> _blogs = new();

    protected override async Task SeedData(IServiceProvider serviceProvider)
    {
        _blogs.AddRange(new List<Blog>()
        {
            new Blog { Id = Guid.NewGuid(), AuthorId = _authors[0].Id, Name = "Author 0 - Blog 0" },
            new Blog { Id = Guid.NewGuid(), AuthorId = _authors[0].Id, Name = "Author 0 - Blog 1" },
            new Blog { Id = Guid.NewGuid(), AuthorId = _authors[1].Id, Name = "Author 1 - Blog 0" },
            new Blog { Id = Guid.NewGuid(), AuthorId = _authors[1].Id, Name = "Author 1 - Blog 1" },
            new Blog { Id = Guid.NewGuid(), AuthorId = _authors[1].Id, Name = "Author 1 - Blog 2" },
            new Blog { Id = Guid.NewGuid(), AuthorId = _authors[2].Id, Name = "Author 2 - Blog 0" },
            new Blog { Id = Guid.NewGuid(), AuthorId = _authors[2].Id, Name = "Author 2 - Blog 1" },
            new Blog { Id = Guid.NewGuid(), AuthorId = _authors[2].Id, Name = "Author 2 - Blog 2" },
            new Blog { Id = Guid.NewGuid(), AuthorId = _authors[2].Id, Name = "Author 2 - Blog 3" },
            new Blog { Id = Guid.NewGuid(), AuthorId = _authors[3].Id, Name = "Author 3 - Blog 0" }
        });

        var repository = serviceProvider.GetRequiredService<IRepository>();
        await using (repository.BeginTransaction())
        {
            await repository.UpsertEntitiesAsync(_authors);
            await repository.UpsertEntitiesAsync(_blogs);
        }
    }

    [TestMethod]
    public async Task AddingEntity_UsingRepositoryChangesetQueue_Works()
    {
        var id = Guid.NewGuid();

        var queue = Services.GetRequiredService<IChangesetQueue>();

        queue.Append(new()
        {
            EntityId = id,
            Type = typeof(Author).AssemblyQualifiedName ?? string.Empty,
            EntityChangeType = EntityChangeType.Add,
            PropertyChangesets =
            {
                new() { Name = nameof(Author.FirstName), Value = "new first name" },
                new() { Name = nameof(Author.LastName), Value = "new last name" }
            }
        });

        queue.Append(new()
        {
            EntityChangeType = EntityChangeType.Barrier
        });

        await queue.ProcessNextBarrier();

        var scopeFactory = Services.GetRequiredService<IServiceScopeFactory>();
        using var scope = scopeFactory.CreateScope();

        var repository = scope.ServiceProvider.GetRequiredService<IRepository>();
        await using (repository.BeginTransaction())
        {
            var author = await repository.GetEntityAsync<Author>(id, CancellationToken.None);

            Assert.IsNotNull(author);
        }
    }

    [TestMethod]
    public async Task ModifyingEntity_UsingRepositoryChangesetQueue_Works()
    {
        var id = _authors[0].Id;

        var queue = Services.GetRequiredService<IChangesetQueue>();

        queue.Append(new()
        {
            EntityId = id,
            Type = typeof(Author).AssemblyQualifiedName ?? string.Empty,
            EntityChangeType = EntityChangeType.Modify,
            PropertyChangesets =
            {
                new() { Name = nameof(Author.LastName), Value = "updated last name" }
            }
        });

        queue.Append(new()
        {
            EntityChangeType = EntityChangeType.Barrier
        });

        await queue.ProcessNextBarrier();

        var scopeFactory = Services.GetRequiredService<IServiceScopeFactory>();
        using var scope = scopeFactory.CreateScope();

        var repository = scope.ServiceProvider.GetRequiredService<IRepository>();
        await using (repository.BeginTransaction())
        {
            var author = await repository.GetEntityAsync<Author>(id, CancellationToken.None);

            Assert.IsNotNull(author);
            Assert.AreEqual(_authors.First(_ => _.Id == id).FirstName, author.FirstName);
            Assert.AreNotEqual(_authors.First(_ => _.Id == id).LastName, author.LastName);
        }
    }

    [TestMethod]
    public async Task DeletingEntity_UsingRepositoryChangesetQueue_Works()
    {
        var id = _authors[0].Id;

        var queue = Services.GetRequiredService<IChangesetQueue>();

        queue.Append(new()
        {
            EntityId = id,
            Type = typeof(Author).AssemblyQualifiedName ?? string.Empty,
            EntityChangeType = EntityChangeType.Delete
        });

        queue.Append(new()
        {
            EntityChangeType = EntityChangeType.Barrier
        });

        await queue.ProcessNextBarrier();

        var scopeFactory = Services.GetRequiredService<IServiceScopeFactory>();
        using var scope = scopeFactory.CreateScope();

        var repository = scope.ServiceProvider.GetRequiredService<IRepository>();
        await using (repository.BeginTransaction())
        {
            var author = await repository.GetEntityAsync<Author>(id, CancellationToken.None);

            Assert.IsNull(author);
        }
    }

    [TestMethod]
    public async Task GetEntitiesAsync_WithoutPendingChanges_RetrievesEntitiesInPersistedState()
    {
        var queue = Services.GetRequiredService<IChangesetQueue>();

        var authors = await queue.GetEntitiesAsync<Author>(CancellationToken.None);

        Assert.AreEqual(_authors.Count, authors.Count);
        Assert.IsTrue(_authors.All(_ => authors.Any(__ => __.Id == _.Id)));
    }

    [TestMethod]
    public async Task GetEntitiesAsync_WithPendingChanges_RetrievesEntitiesInPersistedState_WithChangesApplied()
    {
        var queue = Services.GetRequiredService<IChangesetQueue>();

        var originalAuthor = _authors[1];
        var modifiedAuthor = JsonSerializer.Deserialize<Author>(JsonSerializer.Serialize(originalAuthor, SerializationHelpers.CreateStandardSerializationOptions()))!;

        modifiedAuthor.FirstName = "Updated first name";

        queue.Modify(originalAuthor, modifiedAuthor);

        var authors = await queue.GetEntitiesAsync<Author>(CancellationToken.None);

        Assert.AreEqual(_authors.Count, authors.Count);
        Assert.IsTrue(_authors.All(_ => authors.Any(__ => __.Id == _.Id)));
        Assert.IsTrue(authors.Any(_ => _.FirstName == modifiedAuthor.FirstName));
        Assert.IsFalse(authors.Any(_ => _.FirstName == originalAuthor.FirstName));
    }

    [TestMethod]
    public async Task GetEntitiesAsync_WithPendingAdd_RetrievesEntitiesInPersistedState_WithAdditionalEntity()
    {
        var queue = Services.GetRequiredService<IChangesetQueue>();

        queue.Add(new Author
        {
            Id = Guid.NewGuid()
        });

        var authors = await queue.GetEntitiesAsync<Author>(CancellationToken.None);

        Assert.AreEqual(_authors.Count + 1, authors.Count);
        Assert.IsTrue(_authors.All(_ => authors.Any(__ => __.Id == _.Id)));
    }

    [TestMethod]
    public async Task GetEntitiesAsync_WithPendingDeleteChange_DoesNotReturnEntity()
    {
        var queue = Services.GetRequiredService<IChangesetQueue>();

        var originalAuthor = _authors[1];

        queue.Delete<Author>(originalAuthor.Id);

        var authors = await queue.GetEntitiesAsync<Author>(CancellationToken.None);

        Assert.AreEqual(_authors.Count - 1, authors.Count);
        Assert.IsFalse(authors.Any(_ => _.Id == originalAuthor.Id));
    }

    [TestMethod]
    public async Task GetEntitiesAsyncWithPredicate_WithoutPendingChanges_RetrievesEntitiesInPersistedState()
    {
        var queue = Services.GetRequiredService<IChangesetQueue>();

        var authors = await queue.GetEntitiesAsync<Author>(_ => true, CancellationToken.None);

        Assert.AreEqual(_authors.Count, authors.Count);
        Assert.IsTrue(_authors.All(_ => authors.Any(__ => __.Id == _.Id)));
    }

    [TestMethod]
    public async Task GetEntitiesAsyncWithPredicate_WithPendingChanges_RetrievesEntitiesInPersistedState_WithChangesApplied()
    {
        var queue = Services.GetRequiredService<IChangesetQueue>();

        var originalAuthor = _authors[1];
        var modifiedAuthor = JsonSerializer.Deserialize<Author>(JsonSerializer.Serialize(originalAuthor, SerializationHelpers.CreateStandardSerializationOptions()))!;

        modifiedAuthor.FirstName = "Updated first name";

        queue.Modify(originalAuthor, modifiedAuthor);

        var authors = await queue.GetEntitiesAsync<Author>(_ => true, CancellationToken.None);

        Assert.AreEqual(_authors.Count, authors.Count);
        Assert.IsTrue(_authors.All(_ => authors.Any(__ => __.Id == _.Id)));
        Assert.IsTrue(authors.Any(_ => _.FirstName == modifiedAuthor.FirstName));
        Assert.IsFalse(authors.Any(_ => _.FirstName == originalAuthor.FirstName));
    }

    [TestMethod]
    public async Task GetEntitiesAsyncWithPredicate_WithPendingAdd_RetrievesEntitiesInPersistedState_WithAdditionalEntity()
    {
        var queue = Services.GetRequiredService<IChangesetQueue>();

        queue.Add(new Author
        {
            Id = Guid.NewGuid()
        });

        var authors = await queue.GetEntitiesAsync<Author>(_ => true, CancellationToken.None);

        Assert.AreEqual(_authors.Count + 1, authors.Count);
        Assert.IsTrue(_authors.All(_ => authors.Any(__ => __.Id == _.Id)));
    }

    [TestMethod]
    public async Task GetEntitiesAsyncWithPredicate_WithPendingDeleteChange_DoesNotReturnEntity()
    {
        var queue = Services.GetRequiredService<IChangesetQueue>();

        var originalAuthor = _authors[1];

        queue.Delete<Author>(originalAuthor.Id);

        var authors = await queue.GetEntitiesAsync<Author>(_ => true, CancellationToken.None);

        Assert.AreEqual(_authors.Count - 1, authors.Count);
        Assert.IsFalse(authors.Any(_ => _.Id == originalAuthor.Id));
    }

    [TestMethod]
    public async Task GetEntitiesAsyncWithIncludes_RetrievesRelatedEntitiesFromRepository()
    {
        var queue = Services.GetRequiredService<IChangesetQueue>();

        var authors = await queue.GetEntitiesAsync<Author>(_ => _.Blogs!.Any(), nameof(Author.Blogs), CancellationToken.None);

        Assert.IsTrue(authors.Any(_ => _.Blogs != null && _.Blogs.Any()));
    }

    [TestMethod]
    public async Task GetEntitiesAsyncWithIncludeExpression_RetrievesRelatedEntitiesFromRepository()
    {
        var queue = Services.GetRequiredService<IChangesetQueue>();

        var includes = new List<Expression<Func<Author, object>>>()
        {
            _ => _.Blogs!
        };

        var authors = await queue.GetEntitiesAsync<Author>(_ => _.Blogs!.Any(), includes.ToArray(), CancellationToken.None);

        Assert.IsTrue(authors.Any(_ => _.Blogs != null && _.Blogs.Any()));
    }

    [TestMethod]
    public async Task GetEntitiesAsyncWithIncludes_RetrievesFromChangesetQueue_AndRelatedEntitiesFromRepository()
    {
        var queue = Services.GetRequiredService<IChangesetQueue>();

        var originalAuthor = _authors[0];
        var modifiedAuthor = JsonSerializer.Deserialize<Author>(JsonSerializer.Serialize(originalAuthor, SerializationHelpers.CreateStandardSerializationOptions()))!;

        modifiedAuthor.FirstName = "Updated first name";

        queue.Modify(originalAuthor, modifiedAuthor);

        var authors = await queue.GetEntitiesAsync<Author>(_ => _.Blogs!.Any(), nameof(Author.Blogs), CancellationToken.None);

        var updatedAuthor = authors.FirstOrDefault(_ => _.Id == originalAuthor.Id);

        Assert.IsNotNull(updatedAuthor);
        Assert.IsNotNull(updatedAuthor.Blogs);
        Assert.IsTrue(updatedAuthor.Blogs.Any());
    }

    [TestMethod]
    public async Task GetEntityAsync_ById_RetrievesEntitiesInPersistedState_WithChangesApplied()
    {
        var queue = Services.GetRequiredService<IChangesetQueue>();

        var originalAuthor = _authors[0];
        var modifiedAuthor = JsonSerializer.Deserialize<Author>(JsonSerializer.Serialize(originalAuthor, SerializationHelpers.CreateStandardSerializationOptions()))!;

        modifiedAuthor.FirstName = "Updated first name";

        queue.Modify(originalAuthor, modifiedAuthor);

        var author = await queue.GetEntityAsync<Author>(originalAuthor.Id, CancellationToken.None);

        Assert.IsNotNull(author);
        Assert.AreEqual(modifiedAuthor.FirstName, author.FirstName);

    }

    [TestMethod]
    public async Task GetEntityAsync_ById_WithIncludes_RetrievesEntitiesInPersistedState_WithChangesApplied()
    {
        var queue = Services.GetRequiredService<IChangesetQueue>();

        var originalAuthor = _authors[0];
        var modifiedAuthor = JsonSerializer.Deserialize<Author>(JsonSerializer.Serialize(originalAuthor, SerializationHelpers.CreateStandardSerializationOptions()))!;

        modifiedAuthor.FirstName = "Updated first name";

        queue.Modify(originalAuthor, modifiedAuthor);

        var author = await queue.GetEntityAsync<Author>(originalAuthor.Id, nameof(Author.Blogs), CancellationToken.None);

        Assert.IsNotNull(author);
        Assert.AreEqual(modifiedAuthor.FirstName, author.FirstName);
        Assert.IsNotNull(author.Blogs);
        Assert.IsTrue(author.Blogs.Any());
    }

    [TestMethod]
    public async Task GetEntityAsync_ById_WithIncludesExpression_RetrievesEntitiesInPersistedState_WithChangesApplied()
    {
        var queue = Services.GetRequiredService<IChangesetQueue>();

        var originalAuthor = _authors[0];
        var modifiedAuthor = JsonSerializer.Deserialize<Author>(JsonSerializer.Serialize(originalAuthor, SerializationHelpers.CreateStandardSerializationOptions()))!;

        modifiedAuthor.FirstName = "Updated first name";

        var includes = new List<Expression<Func<Author, object>>>()
        {
            _ => _.Blogs!
        };

        queue.Modify(originalAuthor, modifiedAuthor);

        var author = await queue.GetEntityAsync<Author>(originalAuthor.Id, includes.ToArray(), CancellationToken.None);

        Assert.IsNotNull(author);
        Assert.AreEqual(modifiedAuthor.FirstName, author.FirstName);
        Assert.IsNotNull(author.Blogs);
        Assert.IsTrue(author.Blogs.Any());
    }

    [TestMethod]
    public async Task GetEntityAsync_ByPredicate_RetrievesEntitiesInPersistedState_WithChangesApplied()
    {
        var queue = Services.GetRequiredService<IChangesetQueue>();

        var originalAuthor = _authors[0];
        var modifiedAuthor = JsonSerializer.Deserialize<Author>(JsonSerializer.Serialize(originalAuthor, SerializationHelpers.CreateStandardSerializationOptions()))!;

        modifiedAuthor.FirstName = "Updated first name";

        queue.Modify(originalAuthor, modifiedAuthor);

        var author = await queue.GetEntityAsync<Author>(_ => _.Id == originalAuthor.Id, CancellationToken.None);

        Assert.IsNotNull(author);
        Assert.AreEqual(modifiedAuthor.FirstName, author.FirstName);

    }

    [TestMethod]
    public async Task GetEntityAsync_ByPredicate_WithIncludes_RetrievesEntitiesInPersistedState_WithChangesApplied()
    {
        var queue = Services.GetRequiredService<IChangesetQueue>();

        var originalAuthor = _authors[0];
        var modifiedAuthor = JsonSerializer.Deserialize<Author>(JsonSerializer.Serialize(originalAuthor, SerializationHelpers.CreateStandardSerializationOptions()))!;

        modifiedAuthor.FirstName = "Updated first name";

        queue.Modify(originalAuthor, modifiedAuthor);

        var author = await queue.GetEntityAsync<Author>(_ => _.Id == originalAuthor.Id, nameof(Author.Blogs), CancellationToken.None);

        Assert.IsNotNull(author);
        Assert.AreEqual(modifiedAuthor.FirstName, author.FirstName);
        Assert.IsNotNull(author.Blogs);
        Assert.IsTrue(author.Blogs.Any());
    }

    [TestMethod]
    public async Task GetEntityAsync_ByPredicate_WithIncludesExpression_RetrievesEntitiesInPersistedState_WithChangesApplied()
    {
        var queue = Services.GetRequiredService<IChangesetQueue>();

        var originalAuthor = _authors[0];
        var modifiedAuthor = JsonSerializer.Deserialize<Author>(JsonSerializer.Serialize(originalAuthor, SerializationHelpers.CreateStandardSerializationOptions()))!;

        modifiedAuthor.FirstName = "Updated first name";

        var includes = new List<Expression<Func<Author, object>>>()
        {
            _ => _.Blogs!
        };

        queue.Modify(originalAuthor, modifiedAuthor);

        var author = await queue.GetEntityAsync<Author>(_ => _.Id == originalAuthor.Id, includes.ToArray(), CancellationToken.None);

        Assert.IsNotNull(author);
        Assert.AreEqual(modifiedAuthor.FirstName, author.FirstName);
        Assert.IsNotNull(author.Blogs);
        Assert.IsTrue(author.Blogs.Any());
    }

    [TestMethod]
    public async Task GetEntitiesAsyncWithIncludeExpressionAndProjection_RetrievesRelatedEntitiesFromRepository()
    {
        var queue = Services.GetRequiredService<IChangesetQueue>();

        var includes = new List<Expression<Func<Author, object>>>()
        {
            _ => _.Blogs!
        };

        var authors = await queue.GetEntitiesAsync(_ => _.Blogs!.Any(), includes.ToArray(), (Author a) => new { a.Id, a.Blogs }, CancellationToken.None);

        Assert.IsTrue(authors.Any(_ => _.Blogs != null && _.Blogs.Any()));
    }

    [TestMethod]
    public async Task GetEntitiesAsyncWithIncludesAndProjection_RetrievesFromChangesetQueue_AndRelatedEntitiesFromRepository()
    {
        var queue = Services.GetRequiredService<IChangesetQueue>();

        var originalAuthor = _authors[0];
        var modifiedAuthor = JsonSerializer.Deserialize<Author>(JsonSerializer.Serialize(originalAuthor, SerializationHelpers.CreateStandardSerializationOptions()))!;

        modifiedAuthor.FirstName = "Updated first name";

        queue.Modify(originalAuthor, modifiedAuthor);

        var authors = await queue.GetEntitiesAsync(_ => _.Blogs!.Any(), nameof(Author.Blogs), (Author a) => new { a.Id, a.Blogs }, CancellationToken.None);

        var updatedAuthor = authors.FirstOrDefault(_ => _.Id == originalAuthor.Id);

        Assert.IsNotNull(updatedAuthor);
        Assert.IsNotNull(updatedAuthor.Blogs);
        Assert.IsTrue(updatedAuthor.Blogs.Any());
    }

    [TestMethod]
    public async Task GetEntitiesWithIncludesAndProjection_ReturnsCorrectChildEntitiesOnCorrectParents()
    {
        var queue = Services.GetRequiredService<IChangesetQueue>();

        var authors = await queue.GetEntitiesAsync(_ => true, nameof(Author.Blogs), (Author a) => new { a.Id, a.Blogs }, CancellationToken.None);

        Assert.IsTrue(authors.All(_ => _.Blogs != null && _.Blogs.Any()));
    }
}