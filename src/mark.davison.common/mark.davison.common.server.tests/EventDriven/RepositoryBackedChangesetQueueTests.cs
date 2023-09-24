namespace mark.davison.common.server.tests.EventDriven;

[TestClass]
public class RepositoryBackedChangesetQueueTests
{
    private readonly Mock<IServiceScopeFactory> _serviceScopeFactory;
    private readonly Mock<IRepository> _repository;
    private readonly Mock<IServiceScope> _serviceScope;

    private readonly RepositoryBackedChangesetQueue _queue;

    public RepositoryBackedChangesetQueueTests()
    {
        _serviceScopeFactory = new(MockBehavior.Strict);
        _repository = new(MockBehavior.Strict);
        _serviceScope = new(MockBehavior.Strict);

        _queue = new(_serviceScopeFactory.Object);

        var services = new ServiceCollection();

        services.AddScoped<IRepository>(_ => _repository.Object);

        _serviceScopeFactory.Setup(_ => _.CreateScope()).Returns(() => _serviceScope.Object);
        _serviceScope.Setup(_ => _.Dispose());
        _serviceScope.Setup(_ => _.ServiceProvider).Returns(services.BuildServiceProvider());

        _repository.Setup(_ => _.BeginTransaction()).Returns(() => new TestAsyncDisposable());
    }

    [TestMethod]
    public async Task ProcessChanges_ForSingleAdd_Works()
    {
        _repository
            .Setup(_ => _.UpsertEntityAsync(
                It.IsAny<TestEntity>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((TestEntity e, CancellationToken c) => e)
            .Verifiable();

        _queue.Append(new()
        {
            EntityId = Guid.NewGuid(),
            Type = typeof(TestEntity).AssemblyQualifiedName ?? string.Empty,
            EntityChangeType = EntityChangeType.Add,
            PropertyChangesets =
            {
                new() { Name = nameof(TestEntity.Name), Value = "new name" }
            }
        });

        _queue.Append(new()
        {
            EntityChangeType = EntityChangeType.Barrier
        });

        await _queue.ProcessNextBarrier();

        _repository
            .Verify(
                _ => _.UpsertEntityAsync(
                    It.IsAny<TestEntity>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
    }

    [TestMethod]
    public async Task ProcessChanges_ForSingleModify_Works()
    {
        _repository
            .Setup(_ => _.UpsertEntityAsync(
                It.IsAny<TestEntity>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((TestEntity e, CancellationToken c) => e)
            .Verifiable();

        _repository
            .Setup(_ => _.GetEntityAsync<TestEntity>(
                Guid.Empty,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TestEntity())
            .Verifiable();

        _queue.Append(new()
        {
            EntityId = Guid.Empty,
            Type = typeof(TestEntity).AssemblyQualifiedName ?? string.Empty,
            EntityChangeType = EntityChangeType.Modify,
            PropertyChangesets =
            {
                new() { Name = nameof(TestEntity.Name), Value = "new name" }
            }
        });

        _queue.Append(new()
        {
            EntityChangeType = EntityChangeType.Barrier
        });

        await _queue.ProcessNextBarrier();

        _repository
            .Verify(
                _ => _.GetEntityAsync<TestEntity>(
                    Guid.Empty,
                    It.IsAny<CancellationToken>()),
                Times.Once);

        _repository
            .Verify(
                _ => _.UpsertEntityAsync(
                    It.IsAny<TestEntity>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
    }

    [TestMethod]
    public async Task ProcessChanges_ForSingleDelete_Works()
    {
        _repository
            .Setup(_ => _.DeleteEntityAsync(
                It.IsAny<TestEntity>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((TestEntity e, CancellationToken c) => e)
            .Verifiable();

        _repository
            .Setup(_ => _.GetEntityAsync<TestEntity>(
                Guid.Empty,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TestEntity())
            .Verifiable();

        _queue.Append(new()
        {
            EntityId = Guid.Empty,
            Type = typeof(TestEntity).AssemblyQualifiedName ?? string.Empty,
            EntityChangeType = EntityChangeType.Delete
        });

        _queue.Append(new()
        {
            EntityChangeType = EntityChangeType.Barrier
        });

        await _queue.ProcessNextBarrier();

        _repository
            .Verify(
                _ => _.GetEntityAsync<TestEntity>(
                    Guid.Empty,
                    It.IsAny<CancellationToken>()),
                Times.Once);

        _repository
            .Verify(
                _ => _.DeleteEntityAsync(
                    It.IsAny<TestEntity>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
    }
}
