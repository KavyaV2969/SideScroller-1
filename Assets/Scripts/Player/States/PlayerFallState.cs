public class PlayerFallState : PlayerAiredState
{
    public PlayerFallState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
    }

    public override void Update()
    {
        base.Update();

        // Return to idle once the player touches the ground.
        if (player.groundDetected)
        {
            stateMachine.ChangeState(player.idleState);
        }

        // Start sliding when falling into a wall.
        if (player.wallDetected)
        {
            stateMachine.ChangeState(player.wallSlideState);
        }
    }
}
