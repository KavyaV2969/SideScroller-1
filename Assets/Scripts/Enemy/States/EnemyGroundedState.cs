using UnityEngine;

public class EnemyGroundedState : EnemyState
{
    public EnemyGroundedState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
    }

    public override void Update()
    {
        base.Update();

        //if enemy detects player, statemachine changes to battlestate
        if (enemy.PlayerDetection() == true)
        {
            stateMachine.ChangeState(enemy.battleState);
        }

    }



}
