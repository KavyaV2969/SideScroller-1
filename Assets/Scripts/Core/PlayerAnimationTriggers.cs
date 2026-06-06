using UnityEngine;

public class PlayerAnimationTriggers : MonoBehaviour
{
    private Player player;

    public void Awake()
    {
        player = GetComponentInParent<Player>();
    }

    private void currentStateTrigger()
    {
        player.callAnimationTrigger();
    }
}
