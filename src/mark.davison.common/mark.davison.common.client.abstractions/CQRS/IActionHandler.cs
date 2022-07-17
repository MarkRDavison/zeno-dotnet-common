namespace mark.davison.common.client.abstractions.CQRS;

public interface IActionHandler<in TAction>
    where TAction : class, IAction<TAction>
{

    Task Handle(TAction action, CancellationToken cancellation);

}