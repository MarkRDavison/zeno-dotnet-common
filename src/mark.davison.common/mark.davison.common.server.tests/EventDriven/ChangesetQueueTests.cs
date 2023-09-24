namespace mark.davison.common.server.tests.EventDriven;

[TestClass]
public class ChangesetQueueTests
{
    public class TestChangesetQueue : ChangesetQueue
    {
        public TestChangesetQueue(
            IServiceScopeFactory serviceScopeFactory
        ) : base
        (
            serviceScopeFactory
        )
        {
        }

        public bool ProcessChangesResult { get; set; } = true;
        protected override Task<bool> ProcessChanges(List<EntityChangeset> changes) => Task.FromResult(ProcessChangesResult);
    }


    private readonly TestChangesetQueue _queue;
    private readonly Mock<IServiceScopeFactory> _serviceScopeFactory;

    public ChangesetQueueTests()
    {
        _serviceScopeFactory = new(MockBehavior.Strict);

        _queue = new(_serviceScopeFactory.Object);
    }

    [TestMethod]
    public void HasPeningBarrier_WhenEmpty_ReturnsFalse()
    {
        Assert.IsFalse(_queue.HasPendingBarrier());
    }

    [TestMethod]
    public void Append_WithBarrierToEmptyQueue_DoesNotAppend()
    {
        _queue.Append(new()
        {
            EntityChangeType = EntityChangeType.Barrier
        });

        Assert.IsFalse(_queue.HasPendingBarrier());
    }

    [TestMethod]
    public void Append_WithBarrierToNonEmptyQueue_DoesAppend()
    {
        _queue.Append(new()
        {
            EntityId = Guid.NewGuid(),
            EntityChangeType = EntityChangeType.Add
        });

        _queue.Append(new()
        {
            EntityChangeType = EntityChangeType.Barrier
        });

        Assert.IsTrue(_queue.HasPendingBarrier());
    }

    [TestMethod]
    public void PeekToNextBarrier_WhereNoBarrier_ReturnsEmptyList()
    {
        var response = _queue.PeekToNextBarrier();
        Assert.IsFalse(response.Any());
    }

    [TestMethod]
    public void PopToNextBarrier_WhereNoBarrier_ReturnsEmptyList()
    {
        var response = _queue.PopToNextBarrier();
        Assert.IsFalse(response.Any());
    }

    [TestMethod]
    public void PeekToNextBarrier_WhereBarrier_ReturnsExpectedList()
    {
        _queue.Append(new()
        {
            EntityId = Guid.NewGuid(),
            EntityChangeType = EntityChangeType.Add
        });

        _queue.Append(new()
        {
            EntityChangeType = EntityChangeType.Barrier
        });

        var response = _queue.PeekToNextBarrier();

        Assert.IsTrue(response.Any());

        Assert.IsTrue(_queue.HasPendingBarrier());
    }

    [TestMethod]
    public void PopToNextBarrier_WhereBarrier_ReturnsExpectedList()
    {
        _queue.Append(new()
        {
            EntityId = Guid.NewGuid(),
            EntityChangeType = EntityChangeType.Add
        });

        _queue.Append(new()
        {
            EntityChangeType = EntityChangeType.Barrier
        });

        var response = _queue.PopToNextBarrier();

        Assert.IsTrue(response.Any());

        Assert.IsFalse(_queue.HasPendingBarrier());
    }

    [ExpectedException(typeof(InvalidOperationException))]
    [TestMethod]
    public async Task ProcessNextBarrier_ThrowsIfChangesetCannotBeValidated()
    {
        _queue.Append(new()
        {
            EntityId = Guid.Empty,
            Type = nameof(TestEntity),
            EntityChangeType = EntityChangeType.Delete
        });

        _queue.Append(new()
        {
            EntityId = Guid.Empty,
            Type = nameof(TestEntity),
            EntityChangeType = EntityChangeType.Delete
        });

        _queue.Append(new()
        {
            EntityChangeType = EntityChangeType.Barrier
        });

        await _queue.ProcessNextBarrier();
    }

    [TestMethod]
    public async Task ProcessNextBarrier_RemovesBarrierAfterProcessing()
    {
        _queue.Append(new()
        {
            EntityId = Guid.Empty,
            Type = nameof(TestEntity),
            EntityChangeType = EntityChangeType.Add
        });

        _queue.Append(new()
        {
            EntityId = Guid.Empty,
            Type = nameof(TestEntity),
            EntityChangeType = EntityChangeType.Delete
        });

        _queue.Append(new()
        {
            EntityChangeType = EntityChangeType.Barrier
        });

        await _queue.ProcessNextBarrier();

        Assert.IsFalse(_queue.HasPendingBarrier());
    }

    [ExpectedException(typeof(InvalidOperationException))]
    [TestMethod]
    public async Task ProcessNextBarrier_ThrowsIfProcessChangesFails()
    {
        _queue.Append(new()
        {
            EntityId = Guid.Empty,
            Type = nameof(TestEntity),
            EntityChangeType = EntityChangeType.Add
        });

        _queue.Append(new()
        {
            EntityId = Guid.Empty,
            Type = nameof(TestEntity),
            EntityChangeType = EntityChangeType.Delete
        });

        _queue.Append(new()
        {
            EntityChangeType = EntityChangeType.Barrier
        });
        _queue.ProcessChangesResult = false;

        await _queue.ProcessNextBarrier();
    }

    [TestMethod]
    public async Task ProcessBarrier_DoesNotRemoveBarrier_IfNotFound()
    {
        _queue.Append(new()
        {
            EntityId = Guid.Empty,
            Type = nameof(TestEntity),
            EntityChangeType = EntityChangeType.Add
        });

        _queue.Append(new()
        {
            EntityId = Guid.Empty,
            Type = nameof(TestEntity),
            EntityChangeType = EntityChangeType.Delete
        });

        _queue.Append(new()
        {
            EntityChangeType = EntityChangeType.Barrier
        });

        await _queue.ProcessToBarrier(Guid.NewGuid());

        Assert.IsTrue(_queue.HasPendingBarrier());
    }

    [TestMethod]
    public async Task ProcessNextBarrier_DoesNothingIfNoPendingBarrier()
    {
        await _queue.ProcessNextBarrier();

        Assert.IsFalse(_queue.HasPendingBarrier());
    }

    [TestMethod]
    public async Task Add_CorrectlyAppendsChangeset()
    {
        _queue.Add<TestEntity>(new TestEntity());

        _queue.Append(new()
        {
            EntityChangeType = EntityChangeType.Barrier
        });

        await _queue.ProcessNextBarrier();

        Assert.IsFalse(_queue.HasPendingBarrier());
    }

    [TestMethod]
    public async Task Modify_CorrectlyAppendsChangeset()
    {
        _queue.Modify<TestEntity>(new TestEntity(), new TestEntity());

        _queue.Append(new()
        {
            EntityChangeType = EntityChangeType.Barrier
        });

        await _queue.ProcessNextBarrier();

        Assert.IsFalse(_queue.HasPendingBarrier());
    }

    [TestMethod]
    public async Task Delete_CorrectlyAppendsChangeset()
    {
        _queue.Delete<TestEntity>(Guid.NewGuid());

        _queue.Append(new()
        {
            EntityChangeType = EntityChangeType.Barrier
        });

        await _queue.ProcessNextBarrier();

        Assert.IsFalse(_queue.HasPendingBarrier());
    }
}
