using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class EntityVFX : MonoBehaviour
{
    private SpriteRenderer sr;
    private Entity entity;
    
    [Header("VFX Settings")]
    [SerializeField] private Material onDamageMaterial;
    [SerializeField] private float onDamageVFXDuration = 0.2f;

    [Header("On damage VFX settings")]
    [SerializeField] private Color HitVfxColor = Color.white;

    [SerializeField] GameObject onHitVFX;
    [SerializeField] GameObject onCritHitVFX;

    private Material originalMaterial;
    private Coroutine onDamageVFXCo;

    [Header("VFX Color Settings")]
    [SerializeField] private Color chillVfx = Color.cyan;
    [SerializeField] private Color burnVfx = Color.orange;
    [SerializeField] private Color shockVfx = Color.yellow;
    private Color defaultHitVfxColor;

    private void Awake()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        originalMaterial = sr.material;
        defaultHitVfxColor = HitVfxColor;
        entity = GetComponent<Entity>();
    }

    public void UpdateHitVfxColor(ElementType element)
    {
        if (element == ElementType.Ice)
            HitVfxColor = chillVfx;
        else if (element == ElementType.Fire)
            HitVfxColor = burnVfx;
        else if (element == ElementType.Lightning)
            HitVfxColor = shockVfx;
        else
            HitVfxColor = defaultHitVfxColor;
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
            vfx.GetComponentInChildren<SpriteRenderer>().color = HitVfxColor;

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
