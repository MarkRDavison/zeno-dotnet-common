namespace mark.davison.example.web.features.Store.StartupUseCase;

[Effect]
public sealed class StartupEffects
{
    private readonly IClientHttpRepository _repository;

    public StartupEffects(
        IClientHttpRepository repository)
    {
        _repository = repository;
    }

    public async Task HandleFetchStartupActionAsync(FetchStartupAction action, IDispatcher dispatcher)
    {
        var queryResponse = await _repository.Get<StartupQueryRequest, StartupQueryResponse>(CancellationToken.None);

        var actionResponse = new FetchStartupActionResponse
        {
            Errors = queryResponse.Errors,
            Warnings = queryResponse.Warnings,
            ActionId = action.ActionId,
            Value = queryResponse.Value
        };

        dispatcher.Dispatch(actionResponse);
    }
}
