using UnityEngine;

public class EntityStats : MonoBehaviour
{
    public Stat maxHealth;
    public OffenseStats offense;
    public DefenseStats defense;

    public virtual float GetMaxHealth()
    {
        float baseHp = maxHealth.GetValue();

        return baseHp;
    }

    public virtual float GetEvasionChance()
    {
        float baseEvasion = defense.evasion.GetValue();
        float totalEvasion = baseEvasion;
        const float maxEvasion = 0.15f;
        return Mathf.Clamp(totalEvasion, 0f, maxEvasion);
    }

    public virtual float GetCritRate()
    {
        return offense.critRate.GetValue();
    }

    public virtual float GetCritDamageMultiplier()
    {
        return offense.critDamage.GetValue();
    }

    public virtual float GetDamageBonus()
    {
        return 0f;
    }

    public virtual float GetDamage(out bool isCrit)
    {
        float totalDamage = offense.damage.GetValue() + GetDamageBonus();

        isCrit = CombatChance.Roll(GetCritRate());

        return isCrit
            ? totalDamage * GetCritDamageMultiplier()
            : totalDamage;
    }
}
