using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraInputAdapter : MonoBehaviour
{
    private Camera camera;
    private InputDataSo InputData;

    [Header("旋转目标")]
    private Transform playerRoot;
    private Transform cameraTarget;

    [Header("视角设置")]
    [SerializeField] private float lookSensitivity = 30f;
    [SerializeField] private float minPitch = -40f; // 往上看最高角度
    [SerializeField] private float maxPitch = 60f;  // 往下看最低角度
    [Label("反转Y")]
    [SerializeField] private bool invertY = false;

    private float currentYaw = 0f;
    private float currentPitch = 0f;

    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    private Cinemachine3rdPersonFollow followComponent;

    private void Awake()
    {
        camera = GetComponent<Camera>();
        if (virtualCamera != null)
            followComponent = virtualCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();

    }

    public void Initialize(InputDataSo inputData,Transform playerRoot,Transform cameraTarget)
    {
        InputData = inputData;
        this.playerRoot = playerRoot;
        this.cameraTarget = cameraTarget;

        if (playerRoot != null)
            currentYaw = playerRoot.eulerAngles.y;
        if (cameraTarget != null)
            currentPitch = cameraTarget.localEulerAngles.x;
    }

    private void LateUpdate()
    {
        Vector2 lookInput= InputData.lookInput;

        currentYaw += lookInput.x * lookSensitivity * Time.deltaTime;

        float pitchDelta = lookInput.y * lookSensitivity * Time.deltaTime;
        currentPitch += invertY ? pitchDelta : -pitchDelta;

        //限制上下转的角度
        currentPitch = Mathf.Clamp(currentPitch, minPitch, maxPitch);

        // 物理执行旋转
        playerRoot.rotation = Quaternion.Euler(0f, currentYaw, 0f);

        cameraTarget.localRotation = Quaternion.Euler(currentPitch, 0f, 0f);

        if (followComponent != null)
        {
            float pitchRatio = Mathf.InverseLerp(minPitch, maxPitch, currentPitch); 
                                                                                   
            float distance = Mathf.Lerp(1.5f, 6.0f, pitchRatio); 
            followComponent.CameraDistance = distance;
        }
    }
}
