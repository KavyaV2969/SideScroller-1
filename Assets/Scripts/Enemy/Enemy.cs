using Unity.VisualScripting;
using UnityEngine;

public class Enemy : Entity
{
    public EnemyIdleState idleState;
    public EnemyMoveState moveState;
    public EnemyAttackState attackState;
    public EnemyBattleState battleState;

    [Header("Battle Details")]
    public float battleMoveSpeed = 3;
    public float attackDistance;

    [Header("Movement Details")]
    public float idleTime = 2f;
    public float moveSpeed = 1.4f;
    [Range(0, 2)]
    public float moveAnimSpeedMultiplier = 1;

    [Header("Player Detection")]
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private Transform playerCheck;
    [SerializeField] private float playerCheckDistance = 10;

    public RaycastHit2D PlayerDetection()
    {
        Vector2 origin = playerCheck != null ? playerCheck.position : transform.position;
        RaycastHit2D hit = Physics2D.Raycast(origin, new Vector2(facingDir, 0), playerCheckDistance, whatIsGround | playerLayer);

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

        Vector3 origin = playerCheck != null ? playerCheck.position : transform.position;
        Gizmos.DrawLine(origin, new Vector3(origin.x + (facingDir * playerCheckDistance), origin.y));

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(origin, new Vector3(origin.x + (facingDir * attackDistance), origin.y));
    }
}
