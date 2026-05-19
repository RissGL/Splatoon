using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class  MoveStateBase:IPlayerState
{
    protected MovementParams parameters;
    public PlayerMovementState stateType { get; protected set; }

    protected Vector3 currentVelocity=Vector3.zero;
    public virtual void OnEnter(MoveSystem moveSystem) 
    {
        parameters = moveSystem.GetParamsForState(stateType);
        currentVelocity=moveSystem.GetHorizontalVelocity();
    }

    public virtual void OnUpdate(MoveSystem moveSystem, float deltaTime) 
    {

    }

    public virtual void OnExit(MoveSystem moveSystem) 
    {
        moveSystem.SetHorizontalVelocity(currentVelocity);
    }

    protected Vector3 HandleInput(MoveSystem moveSystem) 
    {
        Vector2 input = moveSystem.inputData.moveInput;
        Vector3 inputDir = new Vector3(input.x, 0.0f, input.y);

        if (inputDir.magnitude > 1.0f)
        {
            inputDir.Normalize();
        }
        return inputDir;
    }

    protected virtual void HandleAirMove(MoveSystem moveSystem, Vector3 inputDir, float deltaTime)
    {
        float targetSpeed = inputDir.magnitude * parameters.maxSpeed;
        float accel = parameters.acceleration * parameters.airControl;

        Vector3 targetVelocity = inputDir * targetSpeed;

        currentVelocity = Vector3.MoveTowards(currentVelocity, targetVelocity, deltaTime * accel);

        moveSystem.SetHorizontalVelocity(currentVelocity);
    }

    protected virtual void HandleGroundMove(MoveSystem moveSystem,Vector3 inputDir,float deltaTime) 
    {
        float maxSpeed = parameters.maxSpeed;

        float accel = (inputDir.magnitude > 0.1f) ? parameters.acceleration : parameters.deceleration;

        Vector3 targetVelocity = inputDir * maxSpeed;
        currentVelocity = Vector3.MoveTowards(currentVelocity, targetVelocity, deltaTime * accel);

        moveSystem.SetHorizontalVelocity(currentVelocity);
    }
}
