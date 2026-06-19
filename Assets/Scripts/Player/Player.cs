using UnityEngine;
using System.Collections;

public class Player : Entity
{
    public PlayerInputSystem input { get; private set; }

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
    public bool movementEnabled { get; private set; } = true;

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

    protected override void Awake()
    {
        base.Awake();

        // Create player-specific state instances used by the state machine.
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
        input.Player.Movement.performed += OnMovementPerformed;
        input.Player.Movement.canceled += OnMovementCanceled;

        ApplyMovementInputState();
    }

    private void OnDisable()
    {
        input.Player.Movement.performed -= OnMovementPerformed;
        input.Player.Movement.canceled -= OnMovementCanceled;
        input.Disable();
    }

    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(idleState);
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

    public void SetMovementEnabled(bool enabled)
    {
        movementEnabled = enabled;

        if (!movementEnabled)
        {
            moveInput = Vector2.zero;
            SetVelocity(0, rb.linearVelocity.y);
            stateMachine.ChangeState(idleState);
        }

        ApplyMovementInputState();
    }

    public PlayerStateSnapshot BuildStateSnapshot(
        string currentLocationId,
        string[] activeQuestFlags,
        string[] completedQuestFlags,
        string[] inventoryItemIds,
        InventoryItemSnapshot[] inventoryItems)
    {
        Vector3 position = transform.position;

        return new PlayerStateSnapshot
        {
            currentLocationId = currentLocationId,
            playerX = position.x,
            playerY = position.y,
            moveInputX = moveInput.x,
            moveInputY = moveInput.y,
            movementEnabled = movementEnabled,
            activeQuestFlags = activeQuestFlags ?? new string[0],
            completedQuestFlags = completedQuestFlags ?? new string[0],
            inventoryItemIds = inventoryItemIds ?? new string[0],
            inventoryItems = inventoryItems ?? new InventoryItemSnapshot[0]
        };
    }

    private void ApplyMovementInputState()
    {
        if (input == null)
        {
            return;
        }

        if (movementEnabled)
        {
            input.Player.Movement.Enable();
            input.Player.Jump.Enable();
            input.Player.Dash.Enable();
            input.Player.Attack.Enable();
        }
        else
        {
            input.Player.Movement.Disable();
            input.Player.Jump.Disable();
            input.Player.Dash.Disable();
            input.Player.Attack.Disable();
        }
    }

    private void OnMovementPerformed(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        if (!movementEnabled)
        {
            return;
        }

        moveInput = ctx.ReadValue<Vector2>();
    }

    private void OnMovementCanceled(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        moveInput = Vector2.zero;
    }
}
