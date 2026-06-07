using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{
    public PlayerInputSystem input { get; private set; }

    public Animator anim { get; private set; }
    public Rigidbody2D rb { get; private set; }
    public StateMachine stateMachine { get; private set; }
    public PlayerIdleState idleState { get; private set; }
    public PlayerMoveState moveState { get; private set; }
    public PlayerJumpState jumpState { get; private set; }
    public PlayerFallState fallState { get; private set; }
    public PlayerWallSlideState wallSlideState { get; private set; }
    public PlayerWallJumpState wallJumpState { get; private set; }
    public PlayerDashState dashState { get; private set; }
    public PlayerAttackState attackState { get; private set; }  
    public PlayerJumpAttackState jumpAttackState { get; private set; }

    public Vector2 moveInput { get; private set; }

    [Header("Attack Details")]
    public Vector2[] attackVelocity;
    public Vector2 jumpAttackVelocity;
    public float attackVelocityDuration = 0.1f;
    public float comboResetTime = 1;
    private Coroutine queuedAttackCo;

    [Header("Movement Details")]
    public float moveSpeed;
    public float jumpForce = 5;
    public Vector2 wallJumpDir;
    public float dashDuration = 0.25f;
    public float dashSpeed = 20;

    [Range(0, 1)]
    public float inAirMoveMultiplier = 0.5f;

    [Header("Collision Details")]
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private float wallCheckDistance;
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private Transform primaryWallCheck;
    [SerializeField] private Transform secondaryWallCheck;
    public bool groundDetected { get; private set; }
    public bool wallDetected { get; private set; }

    private bool facingRight = true;
    public float facingDir { get; private set; } = 1;
    
    private void Awake()
    {
        // Cache required components and create the state instances used by the state machine.
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
        stateMachine = new StateMachine();
        input = new PlayerInputSystem();

        idleState = new PlayerIdleState(this, stateMachine, "idle");
        moveState = new PlayerMoveState(this, stateMachine, "move");
        jumpState = new PlayerJumpState(this, stateMachine, "jumpFall");
        fallState = new PlayerFallState(this, stateMachine, "jumpFall");
        wallSlideState = new PlayerWallSlideState(this, stateMachine, "wallSlide");
        wallJumpState = new PlayerWallJumpState(this, stateMachine, "jumpFall");
        dashState = new PlayerDashState(this, stateMachine, "dash");
        attackState = new PlayerAttackState(this, stateMachine, "basicAttack");
        jumpAttackState = new PlayerJumpAttackState(this, stateMachine, "jumpAttack");
    }

    private void OnEnable()
    {
        input.Enable();

        // Keep the latest movement input available to every player state.
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
        HandleCollisionDetection();

        stateMachine.UpdateActiveState();
    }

    public void SetVelocity(float xVelocity, float yVelocity)
    {
        rb.linearVelocity = new Vector2(xVelocity, yVelocity);

        HandleFlip(xVelocity);
    }

    public void Flip()
    {
        transform.Rotate(0, 180, 0);
        facingRight = !facingRight;
        facingDir *= -1;
    }

    public void callAnimationTrigger()
    {
        stateMachine.currentState.CallAnimationTrigger();
    }

    private void HandleFlip(float xVelocity)
    {
        if (xVelocity > 0 && !facingRight)
        {
            Flip();
        }
        else if (xVelocity < 0 && facingRight)
        {
            Flip();
        }
    }

    private void HandleCollisionDetection()
    {
        // Raycasts are refreshed each frame so states can react immediately.
        groundDetected = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, whatIsGround);
        wallDetected = Physics2D.Raycast(primaryWallCheck.position, Vector2.right * facingDir, wallCheckDistance, whatIsGround) 
                    && Physics2D.Raycast(secondaryWallCheck.position, Vector2.right * facingDir, wallCheckDistance, whatIsGround);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawLine(transform.position, transform.position + new Vector3(0, -groundCheckDistance));
        Gizmos.DrawLine(primaryWallCheck.position, transform.position + new Vector3(wallCheckDistance * facingDir, 0));
        Gizmos.DrawLine(secondaryWallCheck.position, transform.position + new Vector3(wallCheckDistance * facingDir, 0));
    }

    private IEnumerator EnterAttackStateWithDelayedCo()
    {
        yield return new WaitForEndOfFrame();
        stateMachine.ChangeState(attackState);
    }

    public void EnterAttackStatewithDelay()
    {
        if (queuedAttackCo != null)
        {
            StopCoroutine(queuedAttackCo);
        }

        queuedAttackCo = StartCoroutine(EnterAttackStateWithDelayedCo());
    }
}
