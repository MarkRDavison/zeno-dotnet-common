using mark.davison.common.source.generators.CQRS;

namespace mark.davison.common.client.web.tests;

[UseCQRSClient(typeof(ClientCQRSRootTypeEntities), typeof(ClientCQRSClass))]
public class ClientCQRSClass
{
}
