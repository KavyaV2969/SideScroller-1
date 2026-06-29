using UnityEngine;

public class EntityHealth : MonoBehaviour, IDamageable
{
    [SerializeField] protected float health;
    [SerializeField] protected float maxHp = 100;
    [SerializeField] protected bool isDead;
    [SerializeField] protected bool isInvincible;
    [SerializeField] protected bool isCCImmune;
    [SerializeField] protected bool isKnockbackImmune;
    [SerializeField] protected bool isStatusImmune;

    private EntityVFX entityVFX;
    private Entity entity;

    [Header("On Damage Settings")]
    [SerializeField] private Vector2 onDamageKnockback = new Vector2(1.5f, 2.5f);
    [SerializeField] private Vector2 onHeavyDamageKnockback = new Vector2(7f, 7f);
    [SerializeField] private float knockbackDuration = 0.2f;
    [SerializeField] private float heavyKnockbackDuration = 0.2f;
    [Header("On Heavy Damage Settings")]
    [SerializeField] private float heavyDamageThreshold = 0.3f;

    protected virtual void Awake()
    {
        health = maxHp;
        entity = GetComponent<Entity>() ?? GetComponentInParent<Entity>();
        entityVFX = GetComponent<EntityVFX>();
    }

    public virtual void TakeDamage(float damage, Transform damageDealer)
    {
        if (isInvincible || isDead) return;

        if (!isKnockbackImmune && damageDealer != null)
        {
            Vector2 knockback = CalculateKnockback(damage, damageDealer);
            entity?.RecieveKnockback(knockback, isHeavyDamage(damage) ? heavyKnockbackDuration : knockbackDuration);
        }

        entityVFX?.PlayOnDamageVFX();
        ReduceHp(damage);
    }

    void ReduceHp(float damage)
    {
        health -= damage;

        if (health <= 0)
        {
            Die();
        }
    }

    public virtual void Die()
    {
        isDead = true;
        entity.EntityDeath();
        Debug.Log($"{gameObject.name} has died.");
    }

    private Vector2 CalculateKnockback(float damage, Transform damageDealer)
    {
        int direction = damageDealer.position.x > transform.position.x ? -1 : 1;
        Vector2 knockback = isHeavyDamage(damage) ? onHeavyDamageKnockback : onDamageKnockback;
        knockback.x *= direction;

        return knockback;
    }

    private bool isHeavyDamage(float damage)
    {
        return damage >= maxHp * heavyDamageThreshold;
    }
}
