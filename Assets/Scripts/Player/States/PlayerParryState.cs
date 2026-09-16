using UnityEngine;

public class PlayerParryState : PlayerState
{
    private float startedAt;
    public bool IsActive => Time.time >= startedAt + player.parryStartup
        && Time.time < startedAt + player.parryStartup + player.parryActiveDuration;

    public PlayerParryState(Player player, StateMachine stateMachine, string animBoolName)
        : base(player, stateMachine, animBoolName) { }

    public override void Enter()
    {
        base.Enter();
        startedAt = Time.time;
        player.ResetGuard();
        player.SetVelocity(0, rb.linearVelocity.y);
    }

    public override void Update()
    {
        UpdateAnimationParameters();
        player.SetVelocity(0, rb.linearVelocity.y);
        if (!player.groundDetected || Time.time >= startedAt + player.parryStartup + player.parryActiveDuration)
            stateMachine.ChangeState(player.parryRecoveryState);
    }
}
