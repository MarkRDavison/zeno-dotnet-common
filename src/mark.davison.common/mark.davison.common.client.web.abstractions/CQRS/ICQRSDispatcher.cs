namespace mark.davison.common.client.web.abstractions.CQRS;

public interface ICQRSDispatcher : ICommandDispatcher, IQueryDispatcher, IActionDispatcher
{

}
