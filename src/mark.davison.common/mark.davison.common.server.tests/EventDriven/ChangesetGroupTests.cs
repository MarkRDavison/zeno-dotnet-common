namespace mark.davison.common.server.tests.EventDriven;

[TestClass]
public class ChangesetGroupTests
{
    private readonly Mock<IChangesetQueue> _queue;
    private readonly ChangesetGroup _group;

    public ChangesetGroupTests()
    {
        _queue = new(MockBehavior.Strict);

        _queue
            .Setup(_ => _.Append(It.IsAny<EntityChangeset>()))
            .Verifiable();

        _group = new(_queue.Object);
    }

    [TestMethod]
    public void BeginTransaction_WhereItIsRolledBack_DoesNotAppendToQueue()
    {
        using (var transaction = _group.BeginTransaction())
        {
            _group.Append(new());
            _group.Append(new());
            _group.Append(new());

            transaction.Rollback();
        }

        _queue
            .Verify(
                _ => _.Append(It.IsAny<EntityChangeset>()),
                Times.Never);
    }

    [TestMethod]
    public void BeginTransaction_WhereItIsNotRolledBack_AppendsToQueue()
    {
        using (var transaction = _group.BeginTransaction())
        {
            _group.Append(new());
            _group.Append(new());
            _group.Append(new());
        }

        _queue
            .Verify(
                _ => _.Append(It.IsAny<EntityChangeset>()),
                Times.Exactly(3 + 1));
    }

    [TestMethod]
    public void BeginTransactionRecursively_WhereItIsNotRolledBack_AppendsToQueue()
    {
        using (_group.BeginTransaction())
        {
            _group.Append(new());
            _group.Append(new());
            _group.Append(new());

            using (_group.BeginTransaction())
            {
                _group.Append(new());
                _group.Append(new());
                _group.Append(new());
            }
        }

        _queue
            .Verify(
                _ => _.Append(It.IsAny<EntityChangeset>()),
                Times.Exactly(6 + 1));
    }

    [TestMethod]
    public void BeginTransactionRecursively_WhereItIsRolledBack_DoesNotAppendToQueue()
    {
        using (_group.BeginTransaction())
        {
            _group.Append(new());
            _group.Append(new());
            _group.Append(new());

            using (var transaction = _group.BeginTransaction())
            {
                _group.Append(new());
                _group.Append(new());
                _group.Append(new());

                transaction.Rollback();
            }
        }

        _queue
            .Verify(
                _ => _.Append(It.IsAny<EntityChangeset>()),
                Times.Never);
    }
}
