using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveSystem
{
    private InputDataSo inputData;
    private PlayerRuntimeState runtimeState;
    private MovementParamsSet movementParamsSet;

    public MoveSystem(InputDataSo inputData, PlayerRuntimeState runtimeState)
    {
        this.inputData = inputData;
        this.runtimeState = runtimeState;
    }

    public void SetMovementParamsSet(MovementParamsSet set)
    {
        movementParamsSet = set;
    }

    public MovementParams GetParamsForState(PlayerMovementState state)
    {
        return movementParamsSet.GetMovementParams(state);
    }
}
