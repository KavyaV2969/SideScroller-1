public class GroundedState : PlayerState
{
    public GroundedState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
    }

    public override void Update()
    {
        base.Update();
        if (stateMachine.currentState != this) return;

        if (player.groundDetected && player.BlockHeld)
        {
            stateMachine.ChangeState(player.blockState);
            if (input.Player.Attack.WasPressedThisFrame()) player.StartGuardAttack();
            return;
        }

        // If the player is moving downward, transition into the fall state.
        if (rb.linearVelocity.y < 0)
        {
            stateMachine.ChangeState(player.fallState);
            return;
        }

        // Jump input takes the player out of grounded movement.
        if (input.Player.Jump.WasPerformedThisFrame())
        {
            stateMachine.ChangeState(player.jumpState);
            return;
        }

        // If attack pressed, transition to attack state.
        if (input.Player.Attack.WasPerformedThisFrame())
        {
            stateMachine.ChangeState(player.attackState);
        }
    }
}
