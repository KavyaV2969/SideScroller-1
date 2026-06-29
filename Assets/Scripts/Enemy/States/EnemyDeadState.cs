using UnityEngine;

public class EnemyDeadState : EnemyState
{
    public EnemyDeadState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        anim.enabled = false;

        rb.gravityScale = 12f;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 15f);

        enemy.GetComponent<Collider2D>().enabled = false;

        stateMachine.SwitchOffStateMachine();
    }
}
