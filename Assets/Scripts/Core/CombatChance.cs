using UnityEngine;

public static class CombatChance
{
    public static bool Roll(float chance)
    {
        chance = Mathf.Clamp01(chance);
        return Random.value < chance;
    }
}