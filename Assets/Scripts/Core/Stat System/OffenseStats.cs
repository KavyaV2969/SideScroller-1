using System;
using UnityEngine;

[Serializable]
public class OffenseStats
{
    [Header("Physical Stats")]
    public Stat critRate;
    public Stat critDamage;
    public Stat damage;
    public Stat armorPen;

    [Header("Special Stats")]
    public Stat iceDamage;
    public Stat fireDamage;
    public Stat lightningDamage;
    public Stat icePen;
    public Stat firePen;
    public Stat lightningPen;
    public Stat magicPen;
    public Stat extraArcaneScaling;
}

public enum ElementType
{
    None,
    Physical,
    Fire,
    Ice,
    Lightning,
    True
}
