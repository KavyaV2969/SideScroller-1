using UnityEngine;

public class EnemyHealth : EntityHealth
{
    private Enemy enemy => GetComponent<Enemy>();
    public override void TakeDamage(float damage, Transform damageDealer)
    {
        if (damageDealer.CompareTag("Player"))
        {
            // If the damage dealer is the player, enter battle state
            
            if (enemy != null)
            {
                enemy.TryEnteringBattleState(damageDealer);
            }
        }
        base.TakeDamage(damage, damageDealer);
    }
}
