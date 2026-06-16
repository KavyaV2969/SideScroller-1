using Unity.VisualScripting;
using UnityEngine;

public class Enemy : Entity
{
    public EnemyIdleState idleState;
    public EnemyMoveState moveState;
    public EnemyAttackState attackState;
    public KnightBattleState battleState;

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

    protected Vector3 PlayerCheckOrigin => playerCheck != null ? playerCheck.position : transform.position;

    public RaycastHit2D PlayerDetection()
    {
        RaycastHit2D hit = Physics2D.Raycast(PlayerCheckOrigin, new Vector2(facingDir, 0), playerCheckDistance, whatIsGround | playerLayer);

        if (hit.collider == null || hit.collider.gameObject.layer != LayerMask.NameToLayer("Player"))
        {
            return default;
        }
        
        return hit;
    }


    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        Gizmos.color = Color.yellow;

        Gizmos.DrawLine(PlayerCheckOrigin, new Vector3(PlayerCheckOrigin.x + (facingDir * playerCheckDistance), PlayerCheckOrigin.y));

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(PlayerCheckOrigin, new Vector3(PlayerCheckOrigin.x + (facingDir * attackDistance), PlayerCheckOrigin.y));
    }
}
