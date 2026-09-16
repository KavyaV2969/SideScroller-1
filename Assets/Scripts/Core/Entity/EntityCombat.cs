using System;
using System.Collections.Generic;
using UnityEngine;

public class EntityCombat : MonoBehaviour
{
    private EntityVFX entityVFX;
    private EntityStats entityStats;
    private bool attackActive;
    private bool attackResolved;
    private readonly HashSet<IDamageable> resolvedTargets = new HashSet<IDamageable>();
    // A small reusable buffer avoids allocating a new Collider2D[] every swing.
    private readonly Collider2D[] detectedColliders = new Collider2D[8];

    public void BeginAttack()
    {
        attackActive = true;
        attackResolved = false;
        resolvedTargets.Clear();
    }

    public void EndAttack() => attackActive = false;
    private void OnDisable() => EndAttack();

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

    public bool IsTargetInRange(Collider2D target)
    {
        if (targetCheck == null || target == null)
        {
            return false;
        }

        Vector2 closestPoint = target.ClosestPoint(targetCheck.position);
        return ((Vector2)targetCheck.position - closestPoint).sqrMagnitude <= targetCheckRadius * targetCheckRadius;
    }

    private int GetDetectedColliders()
    {
        if (targetCheck == null)
        {
            return 0;
        }

        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(whatIsTarget);
        return Physics2D.OverlapCircle(targetCheck.position, targetCheckRadius, filter, detectedColliders);
    }

    public void OnDrawGizmos()
    {
        if (targetCheck == null) return;
        Gizmos.DrawWireSphere(targetCheck.position, targetCheckRadius);
    }

    public void PerformAttack(float damageMultiplier = 1f)
    {
        if (!attackActive || attackResolved) return;
        // One resolution per state-entered swing, even if events/colliders repeat.
        // Mark before callbacks so automatic counters cannot re-enter this attack.
        attackResolved = true;
        if (GetComponent<Enemy>() != null)
        {
            // Check region-to-region overlap, including parries whose body is outside
            // the damage query. Enemy attacks are active only at this existing event.
            foreach (var player in FindObjectsByType<Player>())
            {
                var health = player.GetComponent<PlayerHealth>();
                if (health != null && (whatIsTarget.value & (1 << player.gameObject.layer)) != 0
                    && player.TryParry(targetCheck.position, targetCheckRadius))
                    resolvedTargets.Add(health);
            }
        }

        int detectedColliderCount = GetDetectedColliders();
        for (int i = 0; i < detectedColliderCount; i++)
        {
            Collider2D collider = detectedColliders[i];
            IDamageable damageable = collider.GetComponentInParent<IDamageable>();

            if (damageable == null || !resolvedTargets.Add(damageable))
            {
                continue;
            }

            bool isCrit;
            ElementType element;
            float elementalDamage = entityStats.GetBasicAttackElementalDamage(out element);

            bool hit = damageable.TakeDamage(entityStats.GetDamage(out isCrit) * damageMultiplier,
                elementalDamage * damageMultiplier, element, transform);
            if (hit)
            {
                entityVFX.UpdateHitVfxColor(element);
                entityVFX.PlayOnHitVFX(collider.transform, isCrit);
            }
        }
    }
}
