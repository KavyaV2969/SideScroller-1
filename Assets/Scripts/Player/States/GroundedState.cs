public class GroundedState : PlayerState
{
    public GroundedState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
    }

    public override void Update()
    {
        base.Update();

        // If the player is moving downward, transition into the fall state.
        if (rb.linearVelocity.y < 0)
        {
            stateMachine.ChangeState(player.fallState);
        }

        // Jump input takes the player out of grounded movement.
        if (input.Player.Jump.WasPerformedThisFrame())
        {
            stateMachine.ChangeState(player.jumpState);
        }

        // If attack pressed, transition to attack state.
        if (input.Player.Attack.WasPerformedThisFrame())
        {
            stateMachine.ChangeState(player.attackState);
        }
    }
}
