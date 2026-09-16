using UnityEngine;

public class PlayerHealth : EntityHealth
{
    private Player player;

    protected override void Awake()
    {
        base.Awake();
        player = GetComponent<Player>();
    }

    public override bool TakeDamage(float damage, float elementalDamage, ElementType element, Transform damageDealer)
    {
        bool blocked = player != null && player.IsBlocking
            && damageDealer != null && damageDealer.GetComponent<Enemy>() != null;
        float multiplier = blocked ? 1f - player.blockDamageReduction : 1f;
        // A blocked hit must keep the grounded guard usable instead of launching the player.
        bool previousKnockbackImmunity = isKnockbackImmune;
        if (blocked) isKnockbackImmune = true;
        bool hit;
        try { hit = base.TakeDamage(damage * multiplier, elementalDamage * multiplier, element, damageDealer); }
        finally { isKnockbackImmune = previousKnockbackImmunity; }
        if (blocked && hit && !isDead) player.GrantRiposte();
        return hit;
    }
}
