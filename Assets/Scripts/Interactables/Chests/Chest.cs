

using UnityEngine;

public class Chest : InteractableEntity
{
    [Header("Chest Settings")]
    private bool isOpen = false;

    public override void Interact()
    {
        if (!isOpen)
        {
            anim.SetBool("ChestOpen", true);
            isOpen = true;
        }
    }
}
