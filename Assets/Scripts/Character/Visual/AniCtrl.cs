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

    public AniCtrl(Animator animator, InputDataSo inputDataSo, MoveSystem moveSystem)
    {
        this.animator = animator;
        this.inputData = inputDataSo;
        this.moveSystem = moveSystem;
    }

    public abstract void UpdateAnime();
}
