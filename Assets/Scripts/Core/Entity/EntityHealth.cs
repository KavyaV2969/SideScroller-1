using UnityEngine;

public class EntityHealth : MonoBehaviour
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
    [SerializeField] private float knockbackDuration = 0.2f;

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
            Vector2 knockback = CalculateKnockback(damageDealer);
            entity?.RecieveKnockback(knockback, knockbackDuration);
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
        Debug.Log($"{gameObject.name} has died.");
    }

    private Vector2 CalculateKnockback(Transform damageDealer)
    {
        int direction = damageDealer.position.x > transform.position.x ? -1 : 1;
        Vector2 knockback = onDamageKnockback;
        knockback.x *= direction;

        return knockback;
    }
}
