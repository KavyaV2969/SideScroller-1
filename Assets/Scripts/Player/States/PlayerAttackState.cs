using UnityEngine;

public class PlayerAttackState : PlayerState
{
    private float attackVelocityTimer;
    private float lastTimeAttacked;
    private bool comboAttackQueued;
    private const int comboLimit = 3;
    private const int comboIndexStart = 1;
    private int comboIndex = 1;
    private Vector2 attackVelocity;
    private float attackDirection;


    public PlayerAttackState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        comboAttackQueued = false;
        ResetComboIndex();

        attackDirection = player.moveInput.x != 0 ? (int)player.moveInput.x : player.facingDir;

        anim.SetInteger("basicAttackComboIndex", comboIndex);
        GenerateAttackVelocity();

        lastTimeAttacked = Time.time;
    }

    public override void Update()
    {
        base.Update();

        HandleAttackVelocity();

        if (input.Player.Attack.WasPressedThisFrame())
        {
            QueueComboAttack();
        }

        if (triggerCalled)
        {
            if (comboAttackQueued)
            {
                anim.SetBool(animBoolName, false);
                player.EnterAttackStatewithDelay();
            }
            else
            {
                stateMachine.ChangeState(player.idleState);
            }
        }

        
        
    }

    public override void Exit()
    {
        base.Exit();

        comboIndex++;
    }

    private void QueueComboAttack()
    {
        if (comboIndex < comboLimit)
        {
            comboAttackQueued = true;
        }
    }

    private void HandleAttackVelocity()
    {
        attackVelocityTimer -= Time.deltaTime;

        if (attackVelocityTimer < 0)
        {
            player.SetVelocity(0, rb.linearVelocity.y);
        }
    }

    private void GenerateAttackVelocity()
    {
        attackVelocity = player.attackVelocity[comboIndex - 1];

        attackVelocityTimer = player.attackVelocityDuration;
        player.SetVelocity(attackVelocity.x * attackDirection, attackVelocity.y);
    }

    void ResetComboIndex()
    {
        if (Time.time > lastTimeAttacked + player.comboResetTime)
        {
            comboIndex = comboIndexStart;
        }

        if (comboIndex > comboLimit)
        {
                comboIndex = comboIndexStart;
        }
    }
}
