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

        Vector2 input=moveSystem.inputData.moveInput;
        Vector3 inputDir=new Vector3(input.x,0.0f,input.y);

        if (inputDir.magnitude > 1.0f)
        {
            inputDir.Normalize();
        }


        float targetSpeed=inputDir.magnitude*parameters.maxSpeed;
        float accel = parameters.acceleration * parameters.airControl;

        Vector3 targetVelocity = inputDir * targetSpeed;

        currentVelocity = Vector3.MoveTowards(currentVelocity, targetVelocity, deltaTime * accel);

        moveSystem.SetHorizontalVelocity(currentVelocity);
    }
}
