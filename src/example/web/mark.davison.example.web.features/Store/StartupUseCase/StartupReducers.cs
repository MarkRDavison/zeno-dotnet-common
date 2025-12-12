namespace mark.davison.example.web.features.Store.StartupUseCase;

public static class StartupReducers
{
    [ReducerMethod]
    public static StartupState FetchStartupActionResponse(StartupState state, FetchStartupActionResponse response)
    {
        if (response.SuccessWithValue)
        {
            return new StartupState(true, response.Value);
        }

        return state;
    }
}
