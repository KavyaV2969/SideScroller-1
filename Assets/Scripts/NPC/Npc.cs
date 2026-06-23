using UnityEngine;

public abstract class NPC : Entity, IInteractable
{
    [SerializeField] private SpriteRenderer interactSprite;
    [SerializeField] private float interactDistance = 1f;
    [SerializeField] private bool playerInRange;
    [SerializeField] private Transform PlayerCheckOrigin;
    private Transform PlayerTransform;

    private PlayerInputSystem input;

    protected override void Awake()
    {
        base.Awake();

        input = new PlayerInputSystem();
    }

    private void OnEnable()
    {
        input?.Player.Interact.Enable();
    }

    protected override void Start()
    {
        base.Start();

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            PlayerTransform = playerObject.transform;
        }
    }

    private void OnDisable()
    {
        input?.Player.Interact.Disable();
    }

    private void OnDestroy()
    {
        input?.Dispose();
    }

    protected override void Update()
    {
        base.Update();

        IsWithinInteractDistance();

        if (playerInRange && input.Player.Interact.WasPressedThisFrame())
        {
            Interact();
        }

        if (interactSprite == null)
        {
            return;
        }

        if (interactSprite.gameObject.activeSelf && !playerInRange)
        {
            interactSprite.gameObject.SetActive(false);
        }
        else if (!interactSprite.gameObject.activeSelf && playerInRange)
        {
            interactSprite.gameObject.SetActive(true);
        }
    }

    public abstract void Interact();

    private void IsWithinInteractDistance()
    {
        if (PlayerTransform == null)
        {
            return;
        }

        if (Vector2.Distance(transform.position, PlayerTransform.position) < interactDistance)
        {
            playerInRange = true;
        }
        else
        {
            playerInRange = false;
        }
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        if (PlayerCheckOrigin == null)
        {
            return;
        }
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(PlayerCheckOrigin.position, interactDistance);
    }

}
