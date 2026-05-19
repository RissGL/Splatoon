using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquidAni : AniCtrl
{
    public SquidAni(Animator animator, InputDataSo inputDataSo,MoveSystem moveSystem, Transform transform)
        : base(animator, inputDataSo,moveSystem, transform)
    {
    }

    public override void UpdateAnime(float deltaTime)
    {
        Vector3 velocity = moveSystem.characterController.velocity;
        velocity.y = 0;
        Vector3 localVelocity = playerTransform.InverseTransformDirection(velocity);

        float maxSpeed = moveSystem.GetParamsForState(moveSystem.currentState.stateType).maxSpeed;
        if (maxSpeed <= 0.01f) maxSpeed = 1f;

        float horizontal = localVelocity.x / maxSpeed;
        float vertical = localVelocity.z / maxSpeed;

        animator.SetFloat(HORIZONTAL, horizontal, 0.1f, deltaTime);
        animator.SetFloat(VERTICAL, vertical, 0.1f, deltaTime);
    }
}
