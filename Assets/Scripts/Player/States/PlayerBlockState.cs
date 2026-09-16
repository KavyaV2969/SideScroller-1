public class PlayerBlockState : PlayerState
{
    public PlayerBlockState(Player player, StateMachine stateMachine, string animBoolName)
        : base(player, stateMachine, animBoolName) { }

    public override void Enter()
    {
        base.Enter();
        player.ResetGuard();
        player.SetVelocity(0, rb.linearVelocity.y);
    }

    public override void Update()
    {
        UpdateAnimationParameters();
        player.SetVelocity(0, rb.linearVelocity.y);
        if (!player.groundDetected || !player.BlockHeld) { player.FinishGuardAction(); return; }
        if (input.Player.Attack.WasPressedThisFrame()) player.StartGuardAttack();
    }

    public override void Exit()
    {
        base.Exit();
        player.ResetGuard();
    }
}
