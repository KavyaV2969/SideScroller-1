using Unity.VisualScripting;
using UnityEngine;

public class PlayerStats : EntityStats
{
    public PlayerMajorStats playerStats;

    public override float GetMaxHealth()
    {
        float baseHp = maxHealth.GetValue();
        float vitalityBonus = playerStats.vitality.GetValue() * 5;

        return baseHp;
    }

    public override float GetEvasionChance()
    {
        float baseEvasion = defense.evasion.GetValue();
        float dexterityBonus = 0f;

        if (playerStats.dexterity.GetValue() <= 60)
        {
            dexterityBonus = playerStats.dexterity.GetValue() * 0.001f;
        }
        else
        {
            dexterityBonus = 0.06f + (playerStats.dexterity.GetValue() - 60) * 0.0005f;
        }

        float totalEvasion = baseEvasion + dexterityBonus;

        const float maxEvasion = 0.15f;

        return Mathf.Clamp(totalEvasion, 0f, maxEvasion);
    }

    public override float GetDamageBonus()
    {
        float strength = playerStats.strength.GetValue();

        if (strength <= 60)
        {
            return strength;
        }

        return 60 + (strength - 60) * 0.5f;
    }

    public override float GetCritRate()
    {
        float dexterity = playerStats.dexterity.GetValue();
        float baseCritRate = offense.critRate.GetValue();

        float dexterityCritRateBonus = dexterity <= 60
            ? dexterity * 0.0025f
            : 0.15f + (dexterity - 60) * 0.00125f;

        return Mathf.Clamp01(baseCritRate + dexterityCritRateBonus);
    }

    public override float GetCritDamageMultiplier()
    {
        float strength = playerStats.strength.GetValue();
        float baseCritDamageMultiplier = offense.critDamage.GetValue();

        float strengthCritDamageBonus = strength <= 60
            ? strength * 0.005f
            : 0.3f + (strength - 60) * 0.0025f;

        return baseCritDamageMultiplier + strengthCritDamageBonus;
    }
}
