﻿namespace mark.davison.common.persistence.tests;

[TestClass]
public sealed class DbContextBaseTests
{
    private readonly IServiceProvider _serviceProvider;

    public DbContextBaseTests()
    {

        var dbSettings = new DatabaseAppSettings
        {
            CONNECTION_STRING = "RANDOM",
            DATABASE_TYPE = DatabaseType.Sqlite
        };

        var serviceCollection = new ServiceCollection();

        serviceCollection
            .AddLogging()
            .AddDatabase<TestDbContext>(false, dbSettings, typeof(TestDbContext));

        _serviceProvider = serviceCollection.BuildServiceProvider();

    }

    [TestMethod]
    public async Task AddAsyncWorks()
    {
        var cancellationToken = CancellationToken.None;
        var author = new Author
        {
            Id = Guid.NewGuid(),
            FirstName = "First",
            LastName = "Last",
            Created = DateTime.Now,
            LastModified = DateTime.Now
        };

        var dbContext = _serviceProvider.GetRequiredService<IDbContext<TestDbContext>>();

        await dbContext.AddAsync(author, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        var savedAuthor = await dbContext.Set<Author>().FirstOrDefaultAsync(_ => _.Id == author.Id);

        Assert.IsNotNull(savedAuthor);
    }

    [TestMethod]
    public async Task UpsertWorks()
    {
        var cancellationToken = CancellationToken.None;
        var author = new Author
        {
            Id = Guid.NewGuid(),
            FirstName = "First",
            LastName = "Last",
            Created = DateTime.Now,
            LastModified = DateTime.Now
        };

        var dbContext = _serviceProvider.GetRequiredService<IDbContext<TestDbContext>>();

        await dbContext.UpsertEntityAsync(author, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        var savedAuthor = await dbContext.Set<Author>().FirstOrDefaultAsync(_ => _.Id == author.Id);

        Assert.IsNotNull(savedAuthor);
    }

    [TestMethod]
    public async Task UpsertMultipleWorks()
    {
        var cancellationToken = CancellationToken.None;
        var author1 = new Author
        {
            Id = Guid.NewGuid(),
            FirstName = "First",
            LastName = "Last",
            Created = DateTime.Now,
            LastModified = DateTime.Now
        };
        var author2 = new Author
        {
            Id = Guid.NewGuid(),
            FirstName = "First",
            LastName = "Last",
            Created = DateTime.Now,
            LastModified = DateTime.Now
        };

        var dbContext = _serviceProvider.GetRequiredService<IDbContext<TestDbContext>>();

        await dbContext.UpsertEntitiesAsync([author1, author2], cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        var savedAuthor1 = await dbContext.GetByIdAsync<Author>(author1.Id, cancellationToken);

        Assert.IsNotNull(savedAuthor1);
        var savedAuthor2 = await dbContext.GetByIdAsync<Author>(author2.Id, cancellationToken);

        Assert.IsNotNull(savedAuthor2);
    }

    [TestMethod]
    public async Task RemoveWorks()
    {
        var cancellationToken = CancellationToken.None;
        var author = new Author
        {
            Id = Guid.NewGuid(),
            FirstName = "First",
            LastName = "Last",
            Created = DateTime.Now,
            LastModified = DateTime.Now
        };

        var dbContext = _serviceProvider.GetRequiredService<IDbContext<TestDbContext>>();

        await dbContext.AddAsync(author, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        await dbContext.RemoveAsync<Author>(author.Id, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        var savedAuthor = await dbContext.Set<Author>().FirstOrDefaultAsync(_ => _.Id == author.Id);

        Assert.IsNull(savedAuthor);
    }

    [TestMethod]
    public async Task DeleteByIdWorks()
    {
        var cancellationToken = CancellationToken.None;
        var author = new Author
        {
            Id = Guid.NewGuid(),
            FirstName = "First",
            LastName = "Last",
            Created = DateTime.Now,
            LastModified = DateTime.Now
        };

        var dbContext = _serviceProvider.GetRequiredService<IDbContext<TestDbContext>>();

        await dbContext.AddAsync(author, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        await dbContext.DeleteEntityByIdAsync<Author>(author.Id, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        var savedAuthor = await dbContext.GetByIdAsync<Author>(author.Id, CancellationToken.None);

        Assert.IsNull(savedAuthor);
    }

    [TestMethod]
    public async Task DeleteByEntityWorks()
    {
        var cancellationToken = CancellationToken.None;
        var author = new Author
        {
            Id = Guid.NewGuid(),
            FirstName = "First",
            LastName = "Last",
            Created = DateTime.Now,
            LastModified = DateTime.Now
        };

        var dbContext = _serviceProvider.GetRequiredService<IDbContext<TestDbContext>>();

        await dbContext.AddAsync(author, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        await dbContext.DeleteEntityAsync<Author>(author, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        var savedAuthor = await dbContext.GetByIdAsync<Author>(author.Id, CancellationToken.None);

        Assert.IsNull(savedAuthor);
    }

    [TestMethod]
    public async Task DeleteMultipleByIdWorks()
    {
        var cancellationToken = CancellationToken.None;
        var author1 = new Author
        {
            Id = Guid.NewGuid(),
            FirstName = "First",
            LastName = "Last",
            Created = DateTime.Now,
            LastModified = DateTime.Now
        };
        var author2 = new Author
        {
            Id = Guid.NewGuid(),
            FirstName = "First",
            LastName = "Last",
            Created = DateTime.Now,
            LastModified = DateTime.Now
        };

        var dbContext = _serviceProvider.GetRequiredService<IDbContext<TestDbContext>>();

        await dbContext.UpsertEntitiesAsync([author1, author2], cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        await dbContext.DeleteEntitiesByIdAsync<Author>([author1.Id, author2.Id], cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        var savedAuthor1 = await dbContext.GetByIdAsync<Author>(author1.Id, cancellationToken);

        Assert.IsNull(savedAuthor1);
        var savedAuthor2 = await dbContext.GetByIdAsync<Author>(author2.Id, cancellationToken);

        Assert.IsNull(savedAuthor2);
    }

    [TestMethod]
    public async Task DeleteMultipleByEntityWorks()
    {
        var cancellationToken = CancellationToken.None;
        var author1 = new Author
        {
            Id = Guid.NewGuid(),
            FirstName = "First",
            LastName = "Last",
            Created = DateTime.Now,
            LastModified = DateTime.Now
        };
        var author2 = new Author
        {
            Id = Guid.NewGuid(),
            FirstName = "First",
            LastName = "Last",
            Created = DateTime.Now,
            LastModified = DateTime.Now
        };

        var dbContext = _serviceProvider.GetRequiredService<IDbContext<TestDbContext>>();

        await dbContext.UpsertEntitiesAsync([author1, author2], cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        await dbContext.DeleteEntitiesAsync<Author>([author1, author2], cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        var savedAuthor1 = await dbContext.GetByIdAsync<Author>(author1.Id, cancellationToken);

        Assert.IsNull(savedAuthor1);
        var savedAuthor2 = await dbContext.GetByIdAsync<Author>(author2.Id, cancellationToken);

        Assert.IsNull(savedAuthor2);
    }

    [DataRow(true)]
    [DataRow(false)]
    [DataTestMethod]
    public async Task TransactionWorks(bool completeTransaction)
    {
        var cancellationToken = CancellationToken.None;
        var author = new Author
        {
            Id = Guid.NewGuid(),
            FirstName = "First",
            LastName = "Last",
            Created = DateTime.Now,
            LastModified = DateTime.Now
        };

        var dbContext = _serviceProvider.GetRequiredService<IDbContext<TestDbContext>>();

        using (var t = await dbContext.BeginTransactionAsync(cancellationToken))
        {

            await dbContext.AddAsync(author, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            if (completeTransaction)
            {
                await t.CommitTransactionAsync(cancellationToken);
            }
        }
        var dbContextRead = _serviceProvider.GetRequiredService<IDbContext<TestDbContext>>();

        var savedAuthor = await dbContextRead.Set<Author>().FirstOrDefaultAsync(_ => _.Id == author.Id);

        if (completeTransaction)
        {
            Assert.IsNotNull(savedAuthor);
        }
        else
        {
            Assert.IsNull(savedAuthor);
        }
    }

    [DataRow(true, true)]
    [DataRow(true, false)]
    [DataRow(false, true)]
    [DataRow(false, false)]
    [DataTestMethod]
    public async Task NestedTransactionWorks(bool completeTransaction1, bool completeTransaction2)
    {
        var cancellationToken = CancellationToken.None;
        var author1 = new Author
        {
            Id = Guid.NewGuid(),
            FirstName = "First1",
            LastName = "Last1",
            Created = DateTime.Now,
            LastModified = DateTime.Now
        };
        var author2 = new Author
        {
            Id = Guid.NewGuid(),
            FirstName = "First2",
            LastName = "Last2",
            Created = DateTime.Now,
            LastModified = DateTime.Now
        };

        var dbContext = _serviceProvider.GetRequiredService<IDbContext<TestDbContext>>();

        using (var t1 = await dbContext.BeginTransactionAsync(cancellationToken))
        {
            await dbContext.AddAsync(author1, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            using (var t2 = await dbContext.BeginTransactionAsync(cancellationToken))
            {
                await dbContext.AddAsync(author2, cancellationToken);
                await dbContext.SaveChangesAsync(cancellationToken);

                if (completeTransaction2)
                {
                    await t2.CommitTransactionAsync(cancellationToken);
                }
            }

            if (completeTransaction1)
            {
                await t1.CommitTransactionAsync(cancellationToken);
            }
        }

        var dbContextRead = _serviceProvider.GetRequiredService<IDbContext<TestDbContext>>();

        var savedAuthor1 = await dbContextRead.Set<Author>().FirstOrDefaultAsync(_ => _.Id == author1.Id);
        var savedAuthor2 = await dbContextRead.Set<Author>().FirstOrDefaultAsync(_ => _.Id == author2.Id);

        if (completeTransaction1 && completeTransaction2)
        {
            Assert.IsNotNull(savedAuthor1);
            Assert.IsNotNull(savedAuthor2);
        }
        else
        {
            Assert.IsNull(savedAuthor1);
            Assert.IsNull(savedAuthor2);
        }
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public async Task CommittingTransaction_WhenInnerTransactionAlreadyRolledBack_Throws()
    {
        var cancellationToken = CancellationToken.None;

        var dbContext = _serviceProvider.GetRequiredService<IDbContext<TestDbContext>>();

        using (var t1 = await dbContext.BeginTransactionAsync(cancellationToken))
        {
            using (var t2 = await dbContext.BeginTransactionAsync(cancellationToken))
            {
                await t1.RollbackTransactionAsync(cancellationToken);
            }

            await t1.CommitTransactionAsync(cancellationToken);
        }

    }

    [DataTestMethod]
    public async Task RollingBackOuterTransactionWorks()
    {
        var cancellationToken = CancellationToken.None;
        var author = new Author
        {
            Id = Guid.NewGuid(),
            FirstName = "First1",
            LastName = "Last1",
            Created = DateTime.Now,
            LastModified = DateTime.Now
        };

        var dbContext = _serviceProvider.GetRequiredService<IDbContext<TestDbContext>>();

        using (var t1 = await dbContext.BeginTransactionAsync(cancellationToken))
        {
            using (var t2 = await dbContext.BeginTransactionAsync(cancellationToken))
            {
                await dbContext.AddAsync(author, cancellationToken);
                await dbContext.SaveChangesAsync(cancellationToken);

                await t2.CommitTransactionAsync(cancellationToken);
            }

            await t1.RollbackTransactionAsync(cancellationToken);
        }

        var dbContextRead = _serviceProvider.GetRequiredService<IDbContext<TestDbContext>>();

        var savedAuthor = await dbContextRead.Set<Author>().FirstOrDefaultAsync(_ => _.Id == author.Id);

        Assert.IsNull(savedAuthor);
    }

    [DataTestMethod]
    public async Task RollingBackInnerTransactionWorks()
    {
        var cancellationToken = CancellationToken.None;
        var author = new Author
        {
            Id = Guid.NewGuid(),
            FirstName = "First1",
            LastName = "Last1",
            Created = DateTime.Now,
            LastModified = DateTime.Now
        };

        var dbContext = _serviceProvider.GetRequiredService<IDbContext<TestDbContext>>();

        using (var t1 = await dbContext.BeginTransactionAsync(cancellationToken))
        {
            await dbContext.AddAsync(author, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            using (var t2 = await dbContext.BeginTransactionAsync(cancellationToken))
            {
                await t2.RollbackTransactionAsync(cancellationToken);
            }
        }

        var dbContextRead = _serviceProvider.GetRequiredService<IDbContext<TestDbContext>>();

        var savedAuthor = await dbContextRead.Set<Author>().FirstOrDefaultAsync(_ => _.Id == author.Id);

        Assert.IsNull(savedAuthor);
    }
}
