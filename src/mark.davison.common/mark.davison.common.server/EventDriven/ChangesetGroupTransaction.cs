namespace mark.davison.common.server.EventDriven;

internal class ChangesetGroupTransaction : IChangesetGroupTransaction
{
    private readonly IChangesetGroup _group;
    private bool _disposedValue;

    public ChangesetGroupTransaction(
        IChangesetGroup group
    )
    {
        _group = group;
        TransactionDepth = 1;
    }

    public int TransactionDepth { get; set; }

    public void Rollback() => _group.Rollback();

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                TransactionDepth--;
                if (TransactionDepth == 0)
                {
                    _group.EndTransaction();
                    _disposedValue = true;
                }
            }

        }
    }


    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
