using UnityEngine;

public class EntityCombat : MonoBehaviour
{
    [Header("Target Detection")]
    [SerializeField] private Transform targetCheck;
    [SerializeField] private float targetCheckRadius;
    [SerializeField] private LayerMask whatIsTarget;

    public Collider2D[] GetDetectedColliders()
    {
        return Physics2D.OverlapCircleAll(targetCheck.position, targetCheckRadius, whatIsTarget);
    }

    public void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(targetCheck.position, targetCheckRadius);
    }

    public void PerformAttack()
    {
        foreach (var collider in GetDetectedColliders())
        {
            EntityHealth targetHealth = collider.GetComponent<EntityHealth>();

            targetHealth?.TakeDamage(10f, transform);
        }
    }
}