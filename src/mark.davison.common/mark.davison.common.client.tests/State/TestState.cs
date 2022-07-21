namespace mark.davison.common.client.tests.State;

public class TestState : IState
{
    public int InitCount { get; set; }
    public int StateValue { get; set; }
    public Action? OnInitialise { get; set; }

    public void Initialise()
    {
        StateValue = 0;
        InitCount++;
        OnInitialise?.Invoke();
    }
}