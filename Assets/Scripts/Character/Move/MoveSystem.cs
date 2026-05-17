using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveSystem
{
    private InputDataSo inputData;
    private PlayerRuntimeState runtimeState;
    private MovementParamsSet movementParamsSet;

    public MoveStateBase currentState { get; private set; }
    private Dictionary<PlayerMovementState, MoveStateBase> states;

    public CharacterController characterController { get; private set; }

    public MoveSystem(InputDataSo inputData, PlayerRuntimeState runtimeState,CharacterController characterController)
    {
        this.inputData = inputData;
        this.runtimeState = runtimeState;
        this.characterController = characterController;

        states = new Dictionary<PlayerMovementState, MoveStateBase>() 
        {
            {PlayerMovementState.HumanRun,new HumanRunState() },
            {PlayerMovementState.HumanAir,new HumanAirState() }
        };

        currentState = states[PlayerMovementState.HumanRun];
    }

    public void SetMovementParamsSet(MovementParamsSet set)
    {
        movementParamsSet = set;
    }

    public MovementParams GetParamsForState(PlayerMovementState state)
    {
        return movementParamsSet.GetMovementParams(state);
    }

    public void ChangeState(PlayerMovementState state) 
    {
        currentState=states[state];
    }
}
