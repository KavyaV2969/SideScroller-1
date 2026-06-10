using UnityEngine;

public class PlayerDashState : PlayerState
{
    private float originalGravityScale;
    private float dashDir;

    public PlayerDashState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        dashDir = player.moveInput.x != 0 ? (int)player.moveInput.x : player.facingDir;
        stateTimer = player.dashDuration;

        originalGravityScale = player.rb.gravityScale;
        player.rb.gravityScale = 0;
    }

    public override void Update()
    {
        base.Update();
        cancelDash();
        player.SetVelocity(player.dashSpeed * dashDir, 0);

        // Return to idle when the dash is over.
        if (stateTimer < 0)
        {
            if (player.groundDetected)
            {
                stateMachine.ChangeState(player.idleState);
            } 
            else 
            {
                stateMachine.ChangeState(player.fallState);
            }
        }
    }

    public override void Exit()
    {
        base.Exit();

        player.SetVelocity(0, 0);

        player.rb.gravityScale = originalGravityScale;
    }

    private void cancelDash()
    {
        if (player.wallDetected)
        {
            if (player.groundDetected)
            {
                stateMachine.ChangeState(player.idleState);
            }
            else 
            {
                stateMachine.ChangeState(player.wallSlideState);
            }
        }
    }
}
