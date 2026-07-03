using UnityEngine;

public class VfxAutoController : MonoBehaviour
{
    [SerializeField] private bool autoDestroy = true;
    [SerializeField] private float destroyDelay = 1f;

    public void Awake()
    {
        if (autoDestroy)
        {
            Destroy(gameObject, destroyDelay);
        }
    }
}
