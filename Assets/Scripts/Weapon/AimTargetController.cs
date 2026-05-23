using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimTargetController : MonoBehaviour
{
    [SerializeField] private Transform aimTarget;

    private InputDataSo InputData;

    [Label("最大范围")]
    [SerializeField]private float maxRange= 5f;
    [Label("最小范围")]
    [SerializeField]private float minRange = -5f;

    [Label("速度")]
    [SerializeField]private float speed = 50f;

    private float currentHeight = 0.6f;

    public void Initialize(InputDataSo inputData)
    {
        InputData = inputData;
    }
    private void Update()
    {
        UpdateAimTarget();
    }

    public void UpdateAimTarget()
    {
        if (InputData == null || aimTarget == null) return;

        float lookDelta = InputData.lookInput.y * speed * Time.deltaTime;

        currentHeight += lookDelta;
        currentHeight = Mathf.Clamp(currentHeight, minRange, maxRange);

        Vector3 lp = aimTarget.localPosition;
        lp.y = currentHeight;
        aimTarget.localPosition = lp;
    }
}
