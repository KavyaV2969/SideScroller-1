using Unity.VisualScripting;
using UnityEngine;

public class Enemy : Entity
{
    public EnemyIdleState idleState;
    public EnemyMoveState moveState;
    public EnemyAttackState attackState;
    public EnemyBattleState battleState;
    public EnemyDeadState deadState;

    [Header("Battle Details")]
    public float battleMoveSpeed = 3;
    public float attackDistance;
    public float battleTimeDuration = 10f;

    [Header("Movement Details")]
    public float idleTime = 2f;
    public float moveSpeed = 1.4f;
    [Range(0, 2)]
    public float moveAnimSpeedMultiplier = 1;

    [Header("Player Detection")]
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private Transform playerCheck;
    [SerializeField] private float playerCheckDistance = 10;
    public Transform player { get; private set; }

    protected Vector3 PlayerCheckOrigin => playerCheck != null ? playerCheck.position : transform.position;

    public Transform GetPlayerReference()
    {
        if (player == null)
        {
            player = PlayerDetection().transform;
        }

        return player;
    }
    public void TryEnteringBattleState(Transform player)
    {
        if (stateMachine.currentState == battleState || stateMachine.currentState == attackState)
        {
            return;
        }

        this.player = player;
        stateMachine.ChangeState(battleState);
    }

    public RaycastHit2D PlayerDetection()
    {
        RaycastHit2D hit = Physics2D.Raycast(PlayerCheckOrigin, new Vector2(facingDir, 0), playerCheckDistance, whatIsGround | playerLayer);

        if (hit.collider == null || hit.collider.gameObject.layer != LayerMask.NameToLayer("Player"))
        {
            return default;
        }
        
        return hit;
    }

    public override void EntityDeath()
    {
        base.EntityDeath();

        stateMachine.ChangeState(deadState);
    }

    public void HandlePlayerDeath()
    {
        stateMachine.ChangeState(idleState);
    }


    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        Gizmos.color = Color.yellow;

        Gizmos.DrawLine(PlayerCheckOrigin, new Vector3(PlayerCheckOrigin.x + (facingDir * playerCheckDistance), PlayerCheckOrigin.y));

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(PlayerCheckOrigin, new Vector3(PlayerCheckOrigin.x + (facingDir * attackDistance), PlayerCheckOrigin.y));
    }

    private void OnEnable()
    {
        Player.OnPlayerDeath += HandlePlayerDeath;
    }

    private void OnDisable()
    {
        Player.OnPlayerDeath -= HandlePlayerDeath;
    }
}
