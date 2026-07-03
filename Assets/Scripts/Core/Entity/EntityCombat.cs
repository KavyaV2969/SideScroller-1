using System;
using UnityEngine;

public class EntityCombat : MonoBehaviour
{
    private EntityVFX entityVFX;
    private EntityStats entityStats;

    [Header("Target Detection")]
    [SerializeField] private Transform targetCheck;
    [SerializeField] private float targetCheckRadius;
    [SerializeField] private LayerMask whatIsTarget;

    [Header("Attack Details")]
    [SerializeField] private float critRate;
    [SerializeField] private float critDamage;
    [SerializeField] private float damage;

    public void Awake()
    {
        entityVFX = GetComponent<EntityVFX>();
        entityStats = GetComponent<EntityStats>();

        critRate = entityStats.GetCritRate();
        critDamage = entityStats.GetCritDamageMultiplier();
        damage = entityStats.offense.damage.GetValue() + entityStats.GetDamageBonus();
    }

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
            IDamageable damageable = collider.GetComponent<IDamageable>();

            if (damageable == null)
            {
                continue;
            }

            bool isCrit;

            damageable.TakeDamage(entityStats.GetDamage(out isCrit), transform);
            entityVFX.PlayOnHitVFX(collider.transform, isCrit);
        }
    }
}
