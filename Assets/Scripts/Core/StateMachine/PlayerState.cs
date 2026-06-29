using UnityEngine;

public abstract class PlayerState : EntityState
{
    protected Player player;
    protected PlayerInputSystem input;

    public PlayerState(StateMachine stateMachine, string animBoolName) : base(stateMachine, animBoolName)
    {
    }

    public PlayerState(Player player, StateMachine stateMachine, string animBoolName) : base(stateMachine, animBoolName)
    {
        this.player = player;

        anim = player.anim;
        input = player.input;
        rb = player.rb;
    }

    public override void Update()
    {
        base.Update();
        
        if (input.Player.Dash.WasPressedThisFrame() && CanDash())
        {
            stateMachine.ChangeState(player.dashState);
        }
    }

    public override void UpdateAnimationParameters()
    {
        base.UpdateAnimationParameters();

        anim.SetFloat("yVelocity", rb.linearVelocity.y);
    }


    private bool CanDash()
    {
        if (player.wallDetected)
        {
            return false;
        }

        if (stateMachine.currentState == player.dashState)
        {
            return false;
        }
        
        return true;
    }
}
