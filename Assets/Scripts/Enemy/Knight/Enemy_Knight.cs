using UnityEngine;

public class Enemy_Knight : Enemy
{
    [Header("Knight Battle Details")]
    public float minimumRetreatDistance = 1;
    public Vector2 retreatVelocity;

    protected override void Awake()
    {
        base.Awake();

        idleState = new EnemyIdleState(this, stateMachine, "idle");
        moveState = new EnemyMoveState(this, stateMachine, "move");
        attackState = new EnemyAttackState(this, stateMachine, "attack");
        battleState = new KnightBattleState(this, stateMachine, "battle");
        deadState = new EnemyDeadState(this, stateMachine, "idle");
    }

    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(idleState);
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        Gizmos.color = Color.green;
        Gizmos.DrawLine(PlayerCheckOrigin, new Vector3(PlayerCheckOrigin.x + (facingDir * minimumRetreatDistance), PlayerCheckOrigin.y));
    }
}
