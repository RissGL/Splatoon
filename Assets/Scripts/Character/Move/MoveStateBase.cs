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

    public virtual void OnUpdate(MoveSystem moveSystem, float deltaTime) { }

    public virtual void OnExit(MoveSystem moveSystem) 
    {
        moveSystem.SetHorizontalVelocity(currentVelocity);
    }
}
