using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.UI;

public class EntityHealth : MonoBehaviour, IDamageable
{
    [SerializeField] protected float health;
    [SerializeField] protected float evasion;
    [SerializeField] protected float armor;
    [SerializeField] protected float armorMitigation;
    [SerializeField] protected bool isDead;
    [SerializeField] protected bool isInvincible;
    [SerializeField] protected bool isCCImmune;
    [SerializeField] protected bool isKnockbackImmune;
    [SerializeField] protected bool isStatusImmune;

    [Header("Magic Resistances")]
    [SerializeField] protected float fireResistance;
    [SerializeField] protected float iceResistance;
    [SerializeField] protected float lightningResistance;

    private EntityVFX entityVFX;
    private Entity entity;
    private EntityStats entityStats;
    private Slider healthBar;

    [Header("On Damage Settings")]
    [SerializeField] private Vector2 onDamageKnockback = new Vector2(1.5f, 2.5f);
    [SerializeField] private Vector2 onHeavyDamageKnockback = new Vector2(7f, 7f);
    [SerializeField] private float knockbackDuration = 0.2f;
    [SerializeField] private float heavyKnockbackDuration = 0.2f;

    [Header("On Heavy Damage Settings")]
    [SerializeField] private float heavyDamageThreshold = 0.3f;

    protected virtual void Awake()
    {
        entity = GetComponent<Entity>() ?? GetComponentInParent<Entity>();
        entityVFX = GetComponent<EntityVFX>();
        entityStats = GetComponent<EntityStats>();
        healthBar = GetComponentInChildren<Slider>();

        health = entityStats.GetMaxHealth();
        evasion = entityStats.GetEvasionChance();
        armor = entityStats.GetArmor();
        armorMitigation = 1f - entityStats.GetArmorMitigation();

        fireResistance = entityStats.GetElementalResistance(ElementType.Fire);
        iceResistance = entityStats.GetElementalResistance(ElementType.Ice);
        lightningResistance = entityStats.GetElementalResistance(ElementType.Lightning);

        UpdateHealthbar();
    }

    public virtual bool TakeDamage(float damage, float elementalDamage, ElementType element, Transform damageDealer)
    {
        if (isInvincible || isDead) { return false; }

        if (AttackEvaded())
        {
            Debug.Log($"{gameObject.name} evaded the attack.");
            return false;
        }

        EntityStats attackerStats = damageDealer.GetComponent<EntityStats>();
        float armorPen = attackerStats != null ? attackerStats.GetArmorPenetration() : 0f;
        float elementalResPen = attackerStats != null ? attackerStats.GetElementalResPen(element) : 0f;

        float mitigation = entityStats.GetArmorMitigation(armorPen);
        float mitigatedDamage = damage * (1 - mitigation);

        float magicResistance = entityStats.GetElementalResistance(element);
        float mitigatedResistance = Mathf.Max(0f, magicResistance - elementalResPen);
        float mitigatedElementalDamage = elementalDamage * (1 - mitigatedResistance);

        TakeKnockback(damageDealer, mitigatedDamage);

        ReduceHp(mitigatedDamage+mitigatedElementalDamage);

        Debug.Log(gameObject.name + " took " + mitigatedDamage + " damage from " + damageDealer.name);

        return true;
    }

    private void TakeKnockback(Transform damageDealer, float mitigatedDamage)
    {
        if (!isKnockbackImmune && damageDealer != null)
        {
            Vector2 knockback = CalculateKnockback(mitigatedDamage, damageDealer);
            entity?.RecieveKnockback(knockback, isHeavyDamage(mitigatedDamage) ? heavyKnockbackDuration : knockbackDuration);
        }
    }

    void ReduceHp(float damage)
    {
        entityVFX?.PlayOnDamageVFX();
        health -= damage;
        UpdateHealthbar();

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

    private void UpdateHealthbar()
    {
        healthBar.value = health / entityStats.GetMaxHealth();
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
        return damage >= entityStats.GetMaxHealth() * heavyDamageThreshold;
    }

    protected virtual bool AttackEvaded()
    {
        float evasionChance = entityStats.GetEvasionChance();
        return CombatChance.Roll(evasionChance);
    }
}
