using System;
using UnityEngine;

[Serializable]
public class DefenseStats
{
    [Header("Physical Stats")]
    public Stat armor;
    public Stat evasion;

    [Header("Special Stats")]
    public Stat iceRes;
    public Stat fireRes;
    public Stat lightningRes;
    public Stat magicRes;
}
