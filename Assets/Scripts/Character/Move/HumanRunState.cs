using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanRunState : MoveStateBase
{
    private Vector3 currentVelocity;

    public HumanRunState() 
    {
        stateType=PlayerMovementState.HumanRun;
    }

    public override void OnEnter(MoveSystem moveSystem)
    {
        base.OnEnter(moveSystem);
        currentVelocity = Vector3.zero;
    }

    public override void OnExit(MoveSystem moveSystem)
    {
        base.OnExit(moveSystem);
    }

    public override void OnUpdate(MoveSystem moveSystem,float deltaTime)
    {
        Vector2 input=moveSystem.inputData.moveInput;
        Vector3 inputDir = new Vector3(input.x, 0.0f, input.y);

        if (inputDir.magnitude > 1.0f)
        {
            inputDir.Normalize();
        }

        float maxSpeed = parameters.maxSpeed;

        float accel = (inputDir.magnitude > 0.1f) ? parameters.acceleration : parameters.deceleration;

        Vector3 targetVelocity = inputDir * maxSpeed;
        currentVelocity = Vector3.MoveTowards(currentVelocity, targetVelocity, deltaTime * accel);

        moveSystem.SetHorizontalVelocity(currentVelocity);
    }
}
