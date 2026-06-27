using UnityEngine;
using System.Collections;

public class EntityVFX : MonoBehaviour
{
    private SpriteRenderer sr;
    
    [Header("VFX Settings")]
    [SerializeField] private Material onDamageMaterial;
    [SerializeField] private float onDamageVFXDuration = 0.2f;

    private Material originalMaterial;
    private Coroutine onDamageVFXCo;

    private void Awake()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        originalMaterial = sr.material;
    }

    public void PlayOnDamageVFX()
    {
        if (onDamageVFXCo != null)
        {
            StopCoroutine(onDamageVFXCo);
        }

        onDamageVFXCo = StartCoroutine(OnDamageVFX());
    }

    private IEnumerator OnDamageVFX()
    {
        sr.material = onDamageMaterial;
        yield return new WaitForSeconds(onDamageVFXDuration);
        sr.material = originalMaterial;
    }
}
