using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AniCtrl 
{
    public static string HORIZONTAL = "Horizontal";
    public static string VERTICAL = "Vertical";

    protected Animator animator;
    protected InputDataSo inputData;
    protected MoveSystem moveSystem;
    protected Transform playerTransform;

    public AniCtrl(Animator animator, InputDataSo inputDataSo, MoveSystem moveSystem, Transform transform)
    {
        this.animator = animator;
        this.inputData = inputDataSo;
        this.moveSystem = moveSystem;
        this.playerTransform = transform;
    }

    public abstract void UpdateAnime(float deltaTime);
}
