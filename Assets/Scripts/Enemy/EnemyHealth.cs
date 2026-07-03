using UnityEngine;

public class EnemyHealth : EntityHealth
{
    private Enemy enemy => GetComponent<Enemy>();
    public override bool TakeDamage(float damage, Transform damageDealer)
    {
        bool wasHit = base.TakeDamage(damage, damageDealer);

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
