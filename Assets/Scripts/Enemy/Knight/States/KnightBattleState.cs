using UnityEngine;

public class KnightBattleState : EnemyBattleState
{
    private const float retreatDuration = 0.35f;

    private Enemy_Knight knight;
    private bool isRetreating;

    public KnightBattleState(Enemy_Knight enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        knight = enemy;
    }

    public override void Enter()
    {
        base.Enter();

        isRetreating = false;

        if (ShouldRetreat())
        {
            StartRetreat();
        }
    }

    protected override bool HandleBattleBehavior()
    {
        if (isRetreating)
        {
            if (stateTimer < 0 && knight.groundDetected)
            {
                isRetreating = false;
            }

            return true;
        }

        if (ShouldRetreat())
        {
            StartRetreat();
            return true;
        }

        return false;
    }

    private bool ShouldRetreat() => DistanceToPlayer() < knight.minimumRetreatDistance;

    private void StartRetreat()
    {
        float retreatDirection = -DirectionToMove();

        isRetreating = true;
        stateTimer = retreatDuration;
        rb.linearVelocity = new Vector2(knight.retreatVelocity.x * retreatDirection, knight.retreatVelocity.y);
    }
}
