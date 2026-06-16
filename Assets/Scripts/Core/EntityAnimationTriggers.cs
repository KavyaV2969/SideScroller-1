using UnityEngine;

public class EntityAnimationTriggers : MonoBehaviour
{
    private Entity entity;

    public void Awake()
    {
        entity = GetComponentInParent<Entity>();
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
        Debug.Log("Attack Trigger");
    }
}
