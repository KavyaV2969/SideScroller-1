using System;
using UnityEngine;

public class EnemyBattleState : EnemyState
{
    private Transform player;
    public EnemyBattleState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        if (player == null){
            player = enemy.PlayerDetection().transform;
        }
    }

    public override void Update()
    {
        base.Update();

        if (WithinAttackRange())
        {
            stateMachine.ChangeState(enemy.attackState);
        }
        else
        {
            enemy.SetVelocity(DirectionToMove() * enemy.battleMoveSpeed, rb.linearVelocity.y);
        }
    }

    private bool WithinAttackRange() => DistanceToPlayer() < enemy.attackDistance;
    private float DistanceToPlayer()
    {
        if (player == null)
        {
            return float.MaxValue;
        }

        return Math.Abs(player.position.x - enemy.transform.position.x);
    }

    private float DirectionToMove()
    {
        if (player == null)
        {
            return 0;
        }

        return player.position.x > enemy.transform.position.x ? 1 : -1;
    }
}
