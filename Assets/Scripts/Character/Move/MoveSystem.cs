using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveSystem
{
    public InputDataSo inputData { get; private set; }
    private PlayerRuntimeState runtimeState;
    private MovementParamsSet movementParamsSet;

    public MoveStateBase currentState { get; private set; }
    private Dictionary<PlayerMovementState, MoveStateBase> states;

    public CharacterController characterController { get; private set; }

    private float currentVerticalVelocity;
    private Vector3 currentHorizontalVelocity;

    public MoveSystem(InputDataSo inputData, PlayerRuntimeState runtimeState,
        CharacterController characterController,MovementParamsSet movementParamsSet)
    {
        this.inputData = inputData;
        this.runtimeState = runtimeState;
        this.characterController = characterController;
        this.movementParamsSet = movementParamsSet;

        states = new Dictionary<PlayerMovementState, MoveStateBase>() 
        {
            {PlayerMovementState.HumanRun,new HumanRunState() },
            {PlayerMovementState.HumanAir,new HumanAirState() }
        };

        currentVerticalVelocity = 0.0f;
        currentState = states[PlayerMovementState.HumanRun];
        currentState.OnEnter(this);
    }
    public Vector3 GetHorizontalVelocity()
    {
        return currentHorizontalVelocity;
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
        currentState.OnExit(this);
        currentState=states[state];
        currentState.OnEnter(this);
    }

    public void Update(float deltaTime)
    {
        currentVerticalVelocity += Physics.gravity.y * deltaTime;

        if (characterController.isGrounded && currentVerticalVelocity < 0)
        {
            currentVerticalVelocity = 0;
        }

        currentState.OnUpdate(this, deltaTime);

        Vector3 totalMotion = (currentHorizontalVelocity + Vector3.up * currentVerticalVelocity) * deltaTime;
        characterController.Move(totalMotion);

        if (IsGrounded() && currentVerticalVelocity <= 0)
        {
            if (currentState.stateType == PlayerMovementState.HumanAir
                ||currentState.stateType==PlayerMovementState.SquidAir)
            {
                if (runtimeState.isSquid)
                    ChangeState(PlayerMovementState.SquidSwim);
                else
                    ChangeState(PlayerMovementState.HumanRun);
            }
        }
    }

    public void SetVerticalVelocity(float value) 
    {
        currentVerticalVelocity=value;
    }


    public void SetHorizontalVelocity(Vector3 val)
    {

        currentHorizontalVelocity=val;
    }

    public bool IsGrounded()
    {
        return characterController.isGrounded;
    }
}
