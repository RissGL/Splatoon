using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanAni: AniCtrl
{
    private Transform playerTransform;

    public HumanAni(Animator animator,InputDataSo inputDataSo, MoveSystem moveSystem, Transform transform)
        : base(animator, inputDataSo, moveSystem)
    {
        this.playerTransform = transform;
    }

    public override void UpdateAnime()
    {
        Vector3 velocity = moveSystem.characterController.velocity;

        velocity.y = 0;
        Vector3 localVelocity = playerTransform.InverseTransformDirection(velocity);

        float maxSpeed = moveSystem.GetParamsForState(moveSystem.currentState.stateType).maxSpeed;

        if (maxSpeed <= 0.01f) maxSpeed = 1f;

        float horizontalTarget = localVelocity.x / maxSpeed;
        float verticalTarget = localVelocity.z / maxSpeed;

        float dampTime = 0.1f; // ¹ý¶ÉÊ±¼ä

        animator.SetFloat(HORIZONTAL, horizontalTarget / maxSpeed, dampTime, Time.deltaTime);
        animator.SetFloat(VERTICAL, verticalTarget / maxSpeed, dampTime, Time.deltaTime);
    }
}
