using UnityEngine;

/// <summary>Keeps the distant sky and skyline moving more slowly than the camera.</summary>
public sealed class VampireCityParallax : MonoBehaviour
{
    [SerializeField] private Transform sky;
    [SerializeField] private Transform skyline;
    [SerializeField, Range(0f, 1f)] private float skyFactor = 0.08f;
    [SerializeField, Range(0f, 1f)] private float skylineFactor = 0.18f;

    private Camera gameCamera;
    private float cameraStartX;
    private Vector3 skyStart;
    private Vector3 skylineStart;

    private void Awake()
    {
        gameCamera = Camera.main;
        if (gameCamera == null) return;
        cameraStartX = gameCamera.transform.position.x;
        if (sky != null) skyStart = sky.position;
        if (skyline != null) skylineStart = skyline.position;
    }

    private void LateUpdate()
    {
        if (gameCamera == null) return;
        float travel = gameCamera.transform.position.x - cameraStartX;
        if (sky != null) sky.position = skyStart + Vector3.right * (travel * skyFactor);
        if (skyline != null) skyline.position = skylineStart + Vector3.right * (travel * skylineFactor);
    }
}
