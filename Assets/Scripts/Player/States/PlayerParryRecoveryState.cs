public class PlayerParryRecoveryState : PlayerState
{
    public PlayerParryRecoveryState(Player player, StateMachine stateMachine, string animBoolName)
        : base(player, stateMachine, animBoolName) { }

    public override void Enter()
    {
        base.Enter();
        stateTimer = player.parryRecoveryDuration;
        player.ResetGuard();
    }

    public override void Update()
    {
        // No base PlayerState.Update: dash, movement and re-block cannot cancel recovery.
        stateTimer -= UnityEngine.Time.deltaTime;
        UpdateAnimationParameters();
        player.SetVelocity(0, rb.linearVelocity.y);
        if (stateTimer <= 0) player.FinishGuardAction();
    }
}
