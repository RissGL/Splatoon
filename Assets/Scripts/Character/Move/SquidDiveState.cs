using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZGameFrameWork.Core;

public class SquidDiveState : MoveStateBase
{
    public SquidDiveState()
    {
        stateType = PlayerMovementState.SquidDive;
    }

    public override void OnEnter(MoveSystem moveSystem)
    {
        base.OnEnter(moveSystem);
        EventCenter.TriggerEvent<bool>((int)EventID.OnSquidDiveChange,false);
    }

    public override void OnExit(MoveSystem moveSystem)
    {
        base.OnExit(moveSystem);
        EventCenter.TriggerEvent<bool>((int)EventID.OnSquidDiveChange, true);
    }

    public override void OnUpdate(MoveSystem moveSystem,float deltaTime)
    {
        base.OnUpdate(moveSystem,deltaTime);

        Vector3 inputDir = HandleInput(moveSystem);

        base.HandleGroundMove(moveSystem, inputDir, deltaTime);
    }
}
