using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanAirState : MoveStateBase
{
    public HumanAirState()
    {
        stateType = PlayerMovementState.HumanAir;
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

        base.HandleAirMove(moveSystem,inputDir,deltaTime);
    }
}
