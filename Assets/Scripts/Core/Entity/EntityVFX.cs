using UnityEngine;
using System.Collections;

public class EntityVFX : MonoBehaviour
{
    private SpriteRenderer sr;
    private Entity entity;
    
    [Header("VFX Settings")]
    [SerializeField] private Material onDamageMaterial;
    [SerializeField] private float onDamageVFXDuration = 0.2f;

    [SerializeField] GameObject onHitVFX;
    [SerializeField] GameObject onCritHitVFX;

    private Material originalMaterial;
    private Coroutine onDamageVFXCo;

    private void Awake()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        originalMaterial = sr.material;
        entity = GetComponent<Entity>();
    }

    public void PlayOnDamageVFX()
    {
        if (onDamageVFXCo != null)
        {
            StopCoroutine(onDamageVFXCo);
        }

        onDamageVFXCo = StartCoroutine(OnDamageVFX());
    }

    public void PlayOnHitVFX(Transform target, bool isCrit)
    {
        GameObject prefab = isCrit? onCritHitVFX : onHitVFX;

        if (prefab != null)
        {
            GameObject vfx = Instantiate(prefab, target.position, Quaternion.identity);

            if (entity.facingDir == -1)
            {
                vfx.transform.Rotate(0, 180, 0);
            }
        }
    }

    private IEnumerator OnDamageVFX()
    {
        sr.material = onDamageMaterial;
        yield return new WaitForSeconds(onDamageVFXDuration);
        sr.material = originalMaterial;
    }
}
