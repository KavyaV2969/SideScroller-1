using UnityEngine;
using UnityEngine.Serialization;

public abstract class InteractableEntity : Entity, IInteractable
{
    [SerializeField] private SpriteRenderer interactSprite;
    [SerializeField] private float interactDistance = 1f;
    [SerializeField] private bool playerInRange;
    [SerializeField] private Transform playerCheckOrigin;
    private Transform _playerTransform;

    private PlayerInputSystem _input;

    protected override void Awake()
    {
        base.Awake();

        _input = new PlayerInputSystem();
    }

    private void OnEnable()
    {
        _input?.Player.Interact.Enable();
    }

    protected override void Start()
    {
        base.Start();

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            _playerTransform = playerObject.transform;
        }
    }

    private void OnDisable()
    {
        _input?.Player.Interact.Disable();
    }

    private void OnDestroy()
    {
        _input?.Dispose();
    }

    protected override void Update()
    {
        base.Update();

        IsWithinInteractDistance();

        if (playerInRange && _input.Player.Interact.WasPressedThisFrame())
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
        if (_playerTransform == null)
        {
            return;
        }

        if (Vector2.Distance(transform.position, _playerTransform.position) < interactDistance)
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

        if (playerCheckOrigin == null)
        {
            return;
        }
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(playerCheckOrigin.position, interactDistance);
    }

}
