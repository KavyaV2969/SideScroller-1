using UnityEngine;

public class EnemyHealth : EntityHealth
{
    private Enemy enemy => GetComponent<Enemy>();
    public override bool TakeDamage(float damage, float elementalDamage, ElementType element, Transform damageDealer)
    {
        bool wasHit = base.TakeDamage(damage, elementalDamage, element, damageDealer);

        if (isDead)
        {
            return false;
        }

        if (!wasHit)
        {
            return false;
        }

        if (damageDealer != null && damageDealer.CompareTag("Player"))
        {
            if (enemy != null)
            {
                enemy.TryEnteringBattleState(damageDealer);
            }
        }

        return true;
    }
}
