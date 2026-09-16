using System;
using UnityEngine;

public class EnemyBattleState : EnemyState
{
    protected Transform player;
    private float lastTimeWasInBattle;
    private readonly EntityCombat entityCombat;

    public EnemyBattleState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        entityCombat = enemy.GetComponent<EntityCombat>();
    }

    public override void Enter()
    {
        base.Enter();

        SetLastTimeInBattle();

        player ??= enemy.GetPlayerReference();
    }

    public override void Update()
    {
        base.Update();

        if (enemy.PlayerDetection() == true)
        {
            SetLastTimeInBattle();
        }

        if (BattleOver())
        {
            stateMachine.ChangeState(enemy.idleState);
            return;
        }

        if (HandleBattleBehavior())
        {
            return;
        }

        if (WithinAttackRange() && enemy.PlayerDetection())
        {
            enemy.SetVelocity(0, rb.linearVelocity.y);
            stateMachine.ChangeState(enemy.attackState);
            return;
        }

        if (enemy.wallDetected)
        {
            enemy.SetVelocity(0, rb.linearVelocity.y);
            return;
        }

        enemy.SetVelocity(DirectionToMove() * enemy.battleMoveSpeed, rb.linearVelocity.y);

    }

    private bool WithinAttackRange()
    {
        // Use the damage circle itself as the engagement test. This keeps attack timing
        // correct when the hitbox is repositioned or resized in the Inspector.
        Collider2D playerCollider = player != null ? player.GetComponent<Collider2D>() : null;
        return entityCombat != null && playerCollider != null
            ? entityCombat.IsTargetInRange(playerCollider)
            : DistanceToPlayer() < enemy.attackDistance;
    }

    private bool BattleOver() => Time.time > lastTimeWasInBattle + enemy.battleTimeDuration;

    private void SetLastTimeInBattle() => lastTimeWasInBattle = Time.time;

    protected virtual bool HandleBattleBehavior() => false;

    protected float DistanceToPlayer()
    {
        if (player == null)
        {
            return float.MaxValue;
        }

        return Math.Abs(player.position.x - enemy.transform.position.x);
    }

    protected float DirectionToMove()
    {
        if (player == null)
        {
            return 0;
        }

        return player.position.x > enemy.transform.position.x ? 1 : -1;
    }
}
