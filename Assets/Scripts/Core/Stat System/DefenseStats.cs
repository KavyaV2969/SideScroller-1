using System;
using UnityEngine;

[Serializable]
public class DefenseStats
{
    [Header("Physical Stats")]
    public Stat armor;
    public Stat evasion;
    public Stat armorShred;

    [Header("Special Stats")]
    public Stat iceRes;
    public Stat fireRes;
    public Stat lightningRes;
    public Stat iceResShred;
    public Stat fireResShred;
    public Stat lightningResShred;
    public Stat magicRes;
    public Stat magicResShred;
}
