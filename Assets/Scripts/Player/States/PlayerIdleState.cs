public class PlayerIdleState : GroundedState
{
    public PlayerIdleState(Player player, StateMachine stateMachine, string stateName) : base(player, stateMachine, stateName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        player.SetVelocity(0, rb.linearVelocity.y);
    }

    public override void Update()
    {
        base.Update();

        if (player.moveInput.x == player.facingDir && player.wallDetected)
        {
            return;
        }

        // Start moving when horizontal input is pressed.
        if (player.moveInput.x != 0)
        {
            stateMachine.ChangeState(player.moveState);
        }
    }
}
