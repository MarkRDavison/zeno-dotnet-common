namespace mark.davison.common.client.web.abstractions.CQRS;

public interface ICommandHandler<in TCommand, TCommandResult>
    where TCommand : class, ICommand<TCommand, TCommandResult>
    where TCommandResult : class, new()
{
    Task<TCommandResult> Handle(TCommand command, CancellationToken cancellation);
}