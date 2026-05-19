using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveSystem
{
    public InputDataSo inputData { get; private set; }
    private PlayerRuntimeState runtimeState;
    private MovementParamsSet movementParamsSet;

    public MoveStateBase currentState { get; private set; }
    public CharacterController characterController { get; private set; }

    private float currentVerticalVelocity;
    private Vector3 currentHorizontalVelocity;

    public EnvironmentDetector Detector { get; set; }

    public MoveSystem(InputDataSo inputData, PlayerRuntimeState runtimeState,
        CharacterController characterController,MovementParamsSet movementParamsSet, EnvironmentDetector detector)
    {
        this.inputData = inputData;
        this.runtimeState = runtimeState;
        this.characterController = characterController;
        this.movementParamsSet = movementParamsSet;

        currentVerticalVelocity = 0.0f;
        currentState.OnEnter(this);
        Detector = detector;
    }

    public void SetCurrentState(MoveStateBase state)
    {
        currentState = state;
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

    public void Update(float deltaTime)
    {
        currentVerticalVelocity += Physics.gravity.y * deltaTime;

        if (Detector.IsGrounded && currentVerticalVelocity < 0)
        {
            currentVerticalVelocity = 0;
        }

        currentState.OnUpdate(this, deltaTime);

        Vector3 totalMotion = (currentHorizontalVelocity + Vector3.up * currentVerticalVelocity) * deltaTime;
        characterController.Move(totalMotion);
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
