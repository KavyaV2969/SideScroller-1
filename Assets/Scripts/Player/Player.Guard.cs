using UnityEngine;

public partial class Player
{
    public PlayerBlockState blockState { get; private set; }
    public PlayerParryState parryState { get; private set; }
    public PlayerParryRecoveryState parryRecoveryState { get; private set; }
    public PlayerCounterAttackState counterAttackState { get; private set; }

    [Header("Block / Counterattack")]
    [Range(0f, 1f)] public float blockDamageReduction = 0.75f;
    [Min(1.01f)] public float riposteDamageMultiplier = 2f;
    [Min(1.02f)] public float parryCounterDamageMultiplier = 3f;

    [Header("Parry Timing (seconds)")]
    [Min(0f)] public float parryStartup = 0.05f;
    [Min(0.01f)] public float parryActiveDuration = 0.15f;
    [Min(0.01f)] public float parryRecoveryDuration = 0.3f;
    [Header("Parry Region (local to Player)")]
    public Vector2 parryRegionOffset = new Vector2(0.8f, 0f);
    [Min(0.01f)] public float parryRegionRadius = 0.85f;

    public bool RiposteReady { get; private set; }
    public bool BlockHeld => isActiveAndEnabled && movementEnabled && input.Player.Block.IsPressed();
    public bool IsBlocking => BlockHeld && groundDetected && stateMachine.currentState == blockState;
    public bool IsGuardAction => stateMachine.currentState == blockState
        || stateMachine.currentState == parryState || stateMachine.currentState == parryRecoveryState
        || stateMachine.currentState == counterAttackState;

    private void InitializeGuardStates()
    {
        blockState = new PlayerBlockState(this, stateMachine, "block");
        parryState = new PlayerParryState(this, stateMachine, "parry");
        parryRecoveryState = new PlayerParryRecoveryState(this, stateMachine, "parryRecovery");
        counterAttackState = new PlayerCounterAttackState(this, stateMachine, "counterAttack");
        input.Player.Block.canceled += _ => ResetGuard();
    }

    public void ResetGuard() => RiposteReady = false;

    public void GrantRiposte()
    {
        if (IsBlocking) RiposteReady = true;
    }

    public void StartGuardAttack()
    {
        if (!IsBlocking) return;
        if (RiposteReady) StartCounterAttack(false);
        else stateMachine.ChangeState(parryState);
    }

    public void StartCounterAttack(bool parry)
    {
        ResetGuard();
        counterAttackState.Prepare(parry ? parryCounterDamageMultiplier : riposteDamageMultiplier);
        stateMachine.ChangeState(counterAttackState);
    }

    // Called only at an enemy's existing attack event, while its overlap region is active.
    public bool TryParry(Vector2 attackCenter, float attackRadius)
    {
        if (!isActiveAndEnabled || !movementEnabled || !groundDetected
            || stateMachine.currentState != parryState || !parryState.IsActive) return false;
        Vector2 center = transform.TransformPoint(parryRegionOffset);
        float radius = parryRegionRadius * Mathf.Max(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.y));
        if ((center - attackCenter).sqrMagnitude > (radius + attackRadius) * (radius + attackRadius)) return false;
        StartCounterAttack(true);
        return true;
    }

    public void FinishGuardAction()
    {
        stateMachine.ChangeState(!groundDetected ? (EntityState)fallState
            : BlockHeld ? blockState : idleState);
    }

    // Special damage is a call argument, never a persistent change to combat stats.
    public void OnAttackAnimationEvent(AnimationEvent animationEvent)
    {
        var clip = animationEvent.animatorClipInfo.clip;
        if (clip == null) return;
        if (stateMachine.currentState == counterAttackState && clip.name == "PlayerCounterAttack")
            GetComponent<EntityCombat>().PerformAttack(counterAttackState.DamageMultiplier);
        else if ((stateMachine.currentState == attackState && clip.name.StartsWith("Player_BasicAttack"))
            || (stateMachine.currentState == jumpAttackState && clip.name == "Player_JumpAttackEnd"))
            GetComponent<EntityCombat>().PerformAttack();
    }

    private void OnValidate()
    {
        blockDamageReduction = Mathf.Clamp01(blockDamageReduction);
        riposteDamageMultiplier = Mathf.Max(1.01f, riposteDamageMultiplier);
        parryCounterDamageMultiplier = Mathf.Max(riposteDamageMultiplier + 0.01f, parryCounterDamageMultiplier);
        parryStartup = Mathf.Max(0f, parryStartup);
        parryActiveDuration = Mathf.Max(0.01f, parryActiveDuration);
        parryRecoveryDuration = Mathf.Max(0.01f, parryRecoveryDuration);
        parryRegionRadius = Mathf.Max(0.01f, parryRegionRadius);
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.TransformPoint(parryRegionOffset),
            parryRegionRadius * Mathf.Max(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.y)));
    }
}
