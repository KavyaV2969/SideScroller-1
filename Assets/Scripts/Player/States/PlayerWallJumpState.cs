public class PlayerWallJumpState : EntityState
{
    public PlayerWallJumpState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        player.SetVelocity(player.wallJumpDir.x * -player.facingDir, player.wallJumpDir.y);
    }

    public override void Update()
    {
        base.Update();

        // Transition to falling once the wall jump starts moving downward.
        if (rb.linearVelocity.y < 0)
        {
            stateMachine.ChangeState(player.fallState);
        }

        // Re-enter the wall slide if the player reconnects with a wall.
        if (player.wallDetected)
        {
            stateMachine.ChangeState(player.wallSlideState);
        }
    }
}
