using Unity.VisualScripting;
using UnityEngine;

public class PlayerStats : EntityStats
{
    public PlayerMajorStats playerStats;

    public override float GetHealthBonus()
    {
        float vitality = playerStats.vitality.GetValue();

        float vitalityHealth;

        if (vitality <= 40f)
        {
            vitalityHealth = vitality * 5f;
        }
        else
        {
            vitalityHealth = 200f + (vitality - 40f) * 2.5f;
        }

        return Mathf.Max(0f, vitalityHealth);
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

    public override float GetDamage(out bool isCrit)
    {
        float totalDamage = offense.damage.GetValue() + GetDamageBonus();

        if (!DoesExtraMagicDamage())
        {
            isCrit = CombatChance.Roll(GetCritRate());
        }
        else
        {
            isCrit = false;
        }

        return isCrit
            ? totalDamage * GetCritDamageMultiplier()
            : totalDamage;
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

    public override float GetArmorBonus()
    {
        float vitality = playerStats.vitality.GetValue();

        float vitalityArmor;

        if (vitality <= 40f)
        {
            vitalityArmor = vitality * 0.5f;
        }
        else
        {
            vitalityArmor = 20f + (vitality - 40f) * 0.25f;
        }

        return Mathf.Max(0f, vitalityArmor);
    }

    public bool DoesExtraMagicDamage()
    {
        if (playerStats.arcane.GetValue() > playerStats.dexterity.GetValue() && playerStats.arcane.GetValue() > playerStats.strength.GetValue())
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public override float GetArcaneScaling()
    {
        float arcane = playerStats.arcane.GetValue();

        float statScaling = 0.4f + arcane / 80f;
        statScaling = Mathf.Clamp(statScaling, 0f, 1.4f) + Mathf.Clamp(offense.extraArcaneScaling.GetValue(), 0f, 0.6f);

        return statScaling;
    }

    public override float GetMagicDamageBonus()
    {
        float arcane = playerStats.arcane.GetValue();

        if (arcane <= 60)
        {
            return arcane * 0.7f;
        }
        else
        {
            return 42f + (arcane - 60f) * 0.35f;
        }
    }

    public override float GetBasicAttackElementalDamage(out ElementType element)
    {
        if (!DoesExtraMagicDamage())
        {
            element = ElementType.None;
            return 0f;
        }

        return base.GetBasicAttackElementalDamage(out element);
    }

    public override float GetElementalResistanceBonus()
    {
        float arcane = playerStats.arcane.GetValue();

        if (arcane <= 60)
        {
            return arcane * 0.0025f;
        }
        else
        {
            return 0.15f + (arcane - 60f) * 0.00125f;
        }
    }
}
