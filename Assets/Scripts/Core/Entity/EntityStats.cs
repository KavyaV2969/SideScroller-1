using UnityEngine;

public class EntityStats : MonoBehaviour
{
    public Stat maxHealth;
    public OffenseStats offense;
    public DefenseStats defense;
    public StatusModifiers modifier;

    public float GetMaxHealth()
    {
        float baseHp = maxHealth.GetValue() + GetHealthBonus();

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

    public float GetArmor()
    {
        return defense.armor.GetValue() + GetArmorBonus();
    }

    public virtual float GetDamageBonus()
    {
        return 0f;
    }

    public virtual float GetHealthBonus()
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

    public virtual float GetArmorBonus()
    {
        return 0f;
    }

    public float GetArmorShred()
    {
        return modifier.armorShred.GetValue();
    }

    public float GetArmorPenetration()
    {
        return offense.armorPen.GetValue();
    }

    public float GetArmorMitigation(float armorPen)
    {
        float armor = GetArmor();
        float armorShred = GetArmorShred();

        float reductionMultiplier = Mathf.Clamp((1f - armorShred) * (1f - armorPen), 0, 1);
        float effectiveArmor = armor * reductionMultiplier;

        float mitigation = effectiveArmor / (effectiveArmor + 100f);

        float mitigationCap = 0.85f;
        float finalMitigation = Mathf.Clamp(mitigation, 0f, mitigationCap);

        return finalMitigation;
    }
    public float GetArmorMitigation()
    {
        return GetArmorMitigation(0);
    }

    public virtual float GetBasicAttackElementalDamage(out ElementType element)
    {
        float fireDamage = offense.fireDamage.GetValue();
        float iceDamage = offense.iceDamage.GetValue();
        float lightningDamage = offense.lightningDamage.GetValue();
        float magicDamageBonus = GetMagicDamageBonus();
        float arcaneScaling = GetArcaneScaling();

        float highestElementalDamage = Mathf.Max(fireDamage, iceDamage, lightningDamage);
        element = GetHighestElementDamageType();

        float finalDamage = (highestElementalDamage + magicDamageBonus) * arcaneScaling;
        return finalDamage;
    }

    public virtual float GetMagicDamageBonus()
    {
        return 0f;
    }

    public virtual float GetArcaneScaling()
    {
        return 1f;
    }

    public ElementType GetHighestElementDamageType()
    {
        float fireDamage = offense.fireDamage.GetValue();
        float iceDamage = offense.iceDamage.GetValue();
        float lightningDamage = offense.lightningDamage.GetValue();

        float highest = Mathf.Max(fireDamage, iceDamage, lightningDamage);

        if (highest <= 0f)
        {
            return ElementType.None;
        }

        if (fireDamage >= iceDamage && fireDamage >= lightningDamage)
        {
            return ElementType.Fire;
        }

        if (iceDamage >= fireDamage && iceDamage >= lightningDamage)
        {
            return ElementType.Ice;
        }

        return ElementType.Lightning;
    }

    public float GetElementalResistance(ElementType element)
    {
        float baseResistance = 0f;
        float bonusResistance = GetElementalResistanceBonus() + defense.magicRes.GetValue() - modifier.magicResShred.GetValue();

        switch (element)
        {
            case ElementType.Fire:
                baseResistance = defense.fireRes.GetValue() - GetElementalResShred(element);
                break;
            case ElementType.Ice:
                baseResistance = defense.iceRes.GetValue() - GetElementalResShred(element);
                break;
            case ElementType.Lightning:
                baseResistance = defense.lightningRes.GetValue() - GetElementalResShred(element);
                break;
            case ElementType.True:
                baseResistance = 0f;
                bonusResistance = 0f;
                break;
        }

        float totalResistance = baseResistance + bonusResistance;
        return Mathf.Clamp01(totalResistance);
    }

    public float GetElementalResShred(ElementType element)
    {
        float baseResistanceShred = 0f;

        switch (element)
        {
            case ElementType.Fire:
                baseResistanceShred = modifier.fireResShred.GetValue();
                break;
            case ElementType.Ice:
                baseResistanceShred = modifier.iceResShred.GetValue();
                break;
            case ElementType.Lightning:
                baseResistanceShred = modifier.lightningResShred.GetValue();
                break;
        }

        return Mathf.Clamp01(baseResistanceShred);
    }

    public float GetElementalResPen(ElementType element)
    {
        float baseResistancePen = 0f;

        switch (element)
        {
            case ElementType.Fire:
                baseResistancePen = offense.firePen.GetValue() + offense.magicPen.GetValue();
                break;
            case ElementType.Ice:
                baseResistancePen = offense.icePen.GetValue() + offense.magicPen.GetValue();
                break;
            case ElementType.Lightning:
                baseResistancePen = offense.lightningPen.GetValue() + offense.magicPen.GetValue();
                break;
        }

        return Mathf.Clamp01(baseResistancePen);
    }

    public virtual float GetElementalResistanceBonus()
    {
        return 0f;
    }
}