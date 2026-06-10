public class StateMachine
{
    public PlayerState currentState { get; private set; }

    public void Initialize(PlayerState startingState)
    {
        currentState = startingState;

        currentState.Enter();
    }

    public void ChangeState(PlayerState newState)
    {
        currentState.Exit();

        currentState = newState;

        currentState.Enter();
    }

    public void UpdateActiveState()
    {
        currentState.Update();
    }
}
