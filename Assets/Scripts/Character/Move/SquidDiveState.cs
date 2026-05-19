using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquidDiveState : MoveStateBase
{
    public SquidDiveState()
    {
        stateType = PlayerMovementState.SquidDive;
    }

    public override void OnEnter(MoveSystem moveSystem)
    {
        base.OnEnter(moveSystem);
    }

    public override void OnExit(MoveSystem moveSystem)
    {
        base.OnExit(moveSystem);
    }

    public override void OnUpdate(MoveSystem moveSystem,float deltaTime)
    {
        base.OnUpdate(moveSystem,deltaTime);

        Vector3 inputDir = HandleInput(moveSystem);

        base.HandleGroundMove(moveSystem, inputDir, deltaTime);
    }
}
