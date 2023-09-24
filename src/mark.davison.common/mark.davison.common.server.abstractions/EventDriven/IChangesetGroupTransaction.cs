namespace mark.davison.common.server.abstractions.EventDriven;

public interface IChangesetGroupTransaction : IDisposable
{
    public void Rollback();
}
