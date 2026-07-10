using System;
using UnityEngine;

[Serializable]
public class StatusModifiers
{
    [Header("Physical Stats")]
    public Stat armorShred;

    [Header("Special Stats")]
    public Stat iceResShred;
    public Stat fireResShred;
    public Stat lightningResShred;
    public Stat magicResShred;
}
