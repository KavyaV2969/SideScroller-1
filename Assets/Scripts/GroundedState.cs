using Unity.VisualScripting;
using UnityEngine;

public class GroundedState : EntityState
{
    public GroundedState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
    }

    public override void Update()
    {
        base.Update();

        if (input.Player.Jump.WasPerformedThisFrame())
        {
            stateMachine.ChangeState(player.jumpState);
        }
    }
}
