using UnityEngine;

public class EntityAnimationTriggers : MonoBehaviour
{
    private Entity entity;
    private EntityCombat entityCombat;

    public void Awake()
    {
        entity = GetComponentInParent<Entity>();
        entityCombat = GetComponentInParent<EntityCombat>();
    }

    private void currentStateTrigger()
    {
        if (entity == null)
        {
            Debug.LogWarning($"{nameof(EntityAnimationTriggers)} on {name} could not find an Entity parent.");
            return;
        }

        entity.callAnimationTrigger();
    }

    private void attackTrigger()
    {
        entityCombat.PerformAttack();
    }
}
