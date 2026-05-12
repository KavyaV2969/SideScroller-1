using UnityEngine;

public class PlayerIdleState : GroundedState
{
    public PlayerIdleState(Player player, StateMachine stateMachine, string stateName) : base(player, stateMachine, stateName)
    {

    }

    public override void Update()
    {
        base.Update();

        if (player.moveInput != Vector2.zero)
        {
            stateMachine.ChangeState(player.moveState);
        }
    }
}
