using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    private Camera mainCamera;
    private float lastCameraPosition;
    [SerializeField] private ParallaxLayer[] backgroundLayers;

    private void Awake()
    {
        mainCamera = Camera.main;   
    }

    private void Update()
    {
        float currentCameraPosition = mainCamera.transform.position.x;
        float distanceToMove = currentCameraPosition - lastCameraPosition;
        lastCameraPosition = currentCameraPosition;

        foreach (ParallaxLayer layer in backgroundLayers)
        {
            layer.Move(distanceToMove);
        }
    }

}
