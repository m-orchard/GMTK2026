public class StateMachine
{
    private IState currentState;

    public IState CurrentState => currentState;

    public void ChangeState(IState nextState)
    {
        currentState?.Exit();
        currentState = nextState;
        currentState?.Enter();
    }

    public void Tick()
    {
        currentState?.Tick();
    }
}
