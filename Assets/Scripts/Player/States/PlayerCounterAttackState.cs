public class PlayerCounterAttackState : PlayerState
{
    public float DamageMultiplier { get; private set; } = 1f;
    public PlayerCounterAttackState(Player player, StateMachine stateMachine, string animBoolName)
        : base(player, stateMachine, animBoolName) { }

    public void Prepare(float multiplier) => DamageMultiplier = multiplier;

    public override void Enter()
    {
        base.Enter();
        player.GetComponent<EntityCombat>().BeginAttack();
        player.SetVelocity(0, rb.linearVelocity.y);
    }

    public override void Update()
    {
        UpdateAnimationParameters();
        player.SetVelocity(0, rb.linearVelocity.y);
        if (triggerCalled) player.FinishGuardAction();
    }

    public override void Exit()
    {
        base.Exit();
        player.GetComponent<EntityCombat>().EndAttack();
        DamageMultiplier = 1f;
        player.ResetGuard();
    }
}
