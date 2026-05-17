using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class  MoveStateBase:IPlayerState
{
    protected MovementParams parameters;
    public PlayerMovementState stateType { get; protected set; }
    public virtual void OnEnter(MoveSystem moveSystem) 
    {
        parameters = moveSystem.GetParamsForState(stateType);
    }

    public virtual void OnUpdate(MoveSystem moveSystem, float deltaTime) { }

    public virtual void OnExit(MoveSystem moveSystem) { }
}
