public class PlayerWallSlideState : PlayerState
{
    public PlayerWallSlideState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
    }

    public override void Update()
    {
        base.Update();

        handleWallSlide();

        if (input.Player.Jump.WasPressedThisFrame())
        {
            stateMachine.ChangeState(player.wallJumpState);
        }

        // Leave the wall slide if the player is no longer touching a wall.
        if (!player.wallDetected)
        {
            stateMachine.ChangeState(player.fallState);
        }

        // Landing while sliding returns the player to idle and faces them away from the wall.
        if (player.groundDetected)
        {
            stateMachine.ChangeState(player.idleState);
            player.Flip();
        }
    }

    private void handleWallSlide()
    {
        if (player.moveInput.y < 0)
        {
            player.SetVelocity(player.moveInput.x, rb.linearVelocity.y);
        }
        else
        {
            player.SetVelocity(player.moveInput.x, player.moveInput.y * 0.3f);
        }
    }
}
