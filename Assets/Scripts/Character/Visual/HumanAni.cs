using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanAni
{
    public static string HORIZONTAL = "Horizontal";
    public static string VERTICAL = "Vertical";

    private Animator animator;
    private InputDataSo inputData;

    public HumanAni(Animator animator,InputDataSo inputDataSo)
    {
        this.animator = animator;
        this.inputData = inputDataSo;
    }

    public void Update()
    {
        if (animator == null)
        {
            Debug.LogError("HumanAni: animator ÊÇ null£¡");
            return;
        }
        if (inputData == null)
        {
            Debug.LogError("HumanAni: inputData ÊÇ null£¡");
            return;
        }
        animator.SetFloat(HORIZONTAL, inputData.moveInput.x);
        animator.SetFloat(VERTICAL, inputData.moveInput.y);
    }
}
