using UnityEngine;

public class EnemyMoveState : EnemyGroundedState
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

        if (stateMachine.currentState != this)
        {
            return;
        }

        enemy.SetVelocity(enemy.moveSpeed * enemy.facingDir, rb.linearVelocity.y);

        if (enemy.groundDetected == false || enemy.wallDetected)
            stateMachine.ChangeState(enemy.idleState);
    }
}
