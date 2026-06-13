using UnityEngine;

public class EnemyMoveState : EnemyState
{
    public EnemyMoveState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        if (!enemy.groundDetected || enemy.wallDetected)
        {
            enemy.Flip();
        }
    }

    public override void Update()
    {
        base.Update();

        enemy.SetVelocity(enemy.facingDir * enemy.moveSpeed, rb.linearVelocity.y);

        if (!enemy.groundDetected || enemy.wallDetected)
        {
            stateMachine.ChangeState(enemy.idleState);
        }
    }
}
