using mark.davison.common.persistence.tests.Context;
using mark.davison.common.Repository;
using mark.davison.common.server.abstractions.Repository;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq.Expressions;

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

    public static MemberExpression CreateNestedPropertyExpression(object root, params string[] propertyPath)
    {
        MemberExpression memberExpression = Expression.Property(Expression.Constant(root, root.GetType()), propertyPath[0]);

        foreach (var member in propertyPath.Skip(1))
        {
            memberExpression = Expression.Property(memberExpression, member);
        }

        return memberExpression;
    }

    static LambdaExpression CreateExpression(Type type, IEnumerable<string> propertyName)
    {
        var param = Expression.Parameter(type, "x");
        Expression body = param;
        foreach (var member in propertyName)
        {
            body = Expression.PropertyOrField(body, member);
        }
        return Expression.Lambda(body, param);
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
        query.Include("Blog");
        query.Include("Blog.Author");

        var fetchedPost = await _repository.GetEntityAsync<Post>(
            post.Id,
            query["include"]);

        Assert.IsNotNull(fetchedPost);
        Assert.IsNotNull(fetchedPost.Blog);
        Assert.IsNotNull(fetchedPost.Blog.Author);
    }
}
