using UnityEngine;

public class Player : MonoBehaviour
{
    public PlayerInputSystem input { get; private set; }

    public Animator anim { get; private set; }
    public Rigidbody2D rb { get; private set; }
    public StateMachine stateMachine { get; private set; }
    public PlayerIdleState idleState { get; private set; }
    public PlayerMoveState moveState { get; private set; }

    public Vector2 moveInput { get; private set; }

    [Header("Movement Details")]
    public float moveSpeed;

    private bool facingRight = true;

    private void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
        stateMachine = new StateMachine();
        input = new PlayerInputSystem();

        idleState = new PlayerIdleState(this, stateMachine, "idle");
        moveState = new PlayerMoveState(this, stateMachine, "move");
    }

    private void OnEnable()
    {
        input.Enable();

        input.Player.Movement.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        input.Player.Movement.canceled += ctx => moveInput = Vector2.zero;
    }

    private void OnDisable()
    {
        input.Disable();
    }

    private void Start()
    {
        stateMachine.Initialize(idleState);
    }

    private void Update()
    {
        stateMachine.UpdateActiveState();
    }

    public void SetVelocity(float xVelocity, float yVelocity) { 
        rb.linearVelocity = new Vector2(xVelocity, yVelocity);
        HandleFlip(xVelocity);
    }

    private void Flip()
    {
        transform.Rotate(0, 180, 0);
        facingRight = !facingRight;
    }

    private void HandleFlip(float xVelocity)
    {   if (xVelocity > 0 && !facingRight)
        {
            Flip();
        }
        else if (xVelocity < 0 && facingRight)
        {
            Flip();
        }
    }
}
